#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Common.Framework.Audio.Collection
{
    /// <summary>
    /// Outcome of matching a persisted <see cref="DeviceInfo"/> against this collection.
    /// </summary>
    public enum DeviceMatchKind
    {
        /// <summary>No candidate matched.</summary>
        None = 0,
        /// <summary>Matched on the exact device Id.</summary>
        ExactId,
        /// <summary>Matched on the stable device-instance path (middle Id segment) and type: survives endpoint-GUID churn from driver updates.</summary>
        StablePath,
        /// <summary>Matched on a unique <see cref="DeviceInfo.NameClean"/>.</summary>
        UniqueName,
        /// <summary>Two or more candidates share the stored <see cref="DeviceInfo.NameClean"/>: no match is returned, the caller decides the policy.</summary>
        Ambiguous
    }

    /// <summary>
    /// Result of <see cref="DeviceReadOnlyCollection{T}.Match"/>. The collection performs no
    /// logging and makes no policy decision: ambiguity is reported, not guessed.
    /// </summary>
    public readonly struct MatchResult<T> where T : DeviceInfo
    {
        public DeviceMatchKind Kind { get; }

        /// <summary>The matched device, only valid when <see cref="IsResolved"/>.</summary>
        public T? Device { get; }

        /// <summary>The equally-named candidates, only populated when <see cref="Kind"/> is <see cref="DeviceMatchKind.Ambiguous"/>.</summary>
        public IReadOnlyList<T>? Candidates { get; }

        /// <summary>True when a single device was identified (ExactId, StablePath or UniqueName).</summary>
        public bool IsResolved => Kind is DeviceMatchKind.ExactId or DeviceMatchKind.StablePath or DeviceMatchKind.UniqueName;

        private MatchResult(DeviceMatchKind kind, T? device, IReadOnlyList<T>? candidates)
        {
            Kind = kind;
            Device = device;
            Candidates = candidates;
        }

        public static MatchResult<T> None() => new(DeviceMatchKind.None, null, null);
        public static MatchResult<T> ExactId(T device) => new(DeviceMatchKind.ExactId, device, null);
        public static MatchResult<T> StablePath(T device) => new(DeviceMatchKind.StablePath, device, null);
        public static MatchResult<T> UniqueName(T device) => new(DeviceMatchKind.UniqueName, device, null);
        public static MatchResult<T> Ambiguous(IReadOnlyList<T> candidates) => new(DeviceMatchKind.Ambiguous, null, candidates);
    }

    public class DeviceReadOnlyCollection<T> : IReadOnlyCollection<T> where T : DeviceInfo
    {
        private readonly EDataFlow _dataFlow;
        private readonly Dictionary<string, T> _byId = new();
        private readonly Dictionary<string, List<T>> _byName = new();
        private readonly Dictionary<string, List<T>> _byInstancePath = new(StringComparer.OrdinalIgnoreCase);

        public DeviceReadOnlyCollection(IEnumerable<T> deviceInfos, EDataFlow dataFlow)
        {
            _dataFlow = dataFlow;
            foreach (var item in deviceInfos)
            {
                if (item == null)
                {
                    return;
                }

                // A duplicate of an already-indexed Id would poison the groupings into false ambiguity.
                if (!_byId.TryAdd(item.Id, item))
                {
                    continue;
                }

                Add(_byName, item.NameClean, item);

                var instancePath = GetInstancePath(item.Id);
                if (instancePath != null)
                {
                    Add(_byInstancePath, instancePath, item);
                }
            }
        }

        private static void Add(Dictionary<string, List<T>> dictionary, string key, T item)
        {
            if (!dictionary.TryGetValue(key, out var list))
            {
                dictionary[key] = new List<T> { item };
            }
            else
            {
                list.Add(item);
            }
        }

        /// <summary>
        /// Extract the stable device-instance path (middle segment) from a Windows endpoint id
        /// <c>{0.0.0.00000000}.{device-instance-path}.{endpoint-guid}</c>. The instance path may
        /// itself contain dots, so the segment boundaries are located on the braces of the
        /// enclosing groups, not on separators.
        /// Returns null for ids without an instance path (2-segment form, e.g. virtual devices).
        /// </summary>
        internal static string? GetInstancePath(string? id)
        {
            if (string.IsNullOrEmpty(id) || id[0] != '{')
            {
                return null;
            }

            // Start of the instance path: right after the first "}."
            var start = id.IndexOf("}.", StringComparison.Ordinal) + 2;
            // End of the instance path: right before the trailing ".{endpoint-guid}"
            var end = id.LastIndexOf(".{", StringComparison.Ordinal);
            if (start < 2 || end <= start)
            {
                return null;
            }

            var path = id.Substring(start, end - start);
            // Some Id forms brace the instance path; braces never occur inside it, so strip the delimiters
            // to get a uniform key whether the path is braced or not.
            return path.Length > 1 && path[0] == '{' && path[^1] == '}'
                ? path.Substring(1, path.Length - 2)
                : path;
        }

        /// <summary>
        /// Match a persisted <see cref="DeviceInfo"/> against the devices of this collection,
        /// in tier order: exact Id, then stable device-instance path (with the same type),
        /// then unique <see cref="DeviceInfo.NameClean"/>. Ambiguity is reported, never guessed.
        /// </summary>
        public MatchResult<T> Match(DeviceInfo stored)
        {
            if (stored == null || stored.Type != _dataFlow)
            {
                return MatchResult<T>.None();
            }

            // Tier 0 - ExactId
            if (_byId.TryGetValue(stored.Id, out var exact) && exact.Type == stored.Type)
            {
                return MatchResult<T>.ExactId(exact);
            }

            // Tier 1 - StablePath: the instance path survives endpoint-GUID churn (driver updates)
            var instancePath = GetInstancePath(stored.Id);
            if (instancePath != null && _byInstancePath.TryGetValue(instancePath, out var pathMatches))
            {
                var typed = pathMatches.Where(candidate => candidate.Type == stored.Type).ToList();
                if (typed.Count == 1)
                {
                    return MatchResult<T>.StablePath(typed[0]);
                }

                if (typed.Count > 1)
                {
                    return MatchResult<T>.Ambiguous(typed);
                }
            }

            // Tier 2 - UniqueName: with duplicates present, report ambiguity instead of last-wins
            if (_byName.TryGetValue(stored.NameClean, out var nameMatches))
            {
                var typed = nameMatches.Where(candidate => candidate.Type == stored.Type).ToList();
                if (typed.Count == 1)
                {
                    return MatchResult<T>.UniqueName(typed[0]);
                }

                if (typed.Count > 1)
                {
                    return MatchResult<T>.Ambiguous(typed);
                }
            }

            return MatchResult<T>.None();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _byId.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Intersect with another list of <see cref="DeviceInfo"/>
        /// </summary>
        /// <param name="second"></param>
        /// <returns></returns>
        public IEnumerable<T> IntersectWith(IEnumerable<DeviceInfo> second)
        {
            return second
                   .Where(info => info.Type == _dataFlow)
                   .Select(Match)
                   .Where(result => result.IsResolved)
                   .Select(result => result.Device!)
                   .Distinct();
        }

        public int Count => _byId.Count;
    }
}
