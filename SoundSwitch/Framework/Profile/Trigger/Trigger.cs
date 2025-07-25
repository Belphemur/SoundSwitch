﻿using System;
using SoundSwitch.Framework.WinApi.Keyboard;

namespace SoundSwitch.Framework.Profile.Trigger;

public class Trigger(TriggerFactory.Enum type) : IEquatable<Trigger>
{
    public TriggerFactory.Enum Type { get; } = type;
    public string WindowName { get; set; }
    public string ApplicationPath { get; set; }
    public HotKey HotKey { get; set; }

    /// <summary>
    /// Should this trigger restore the devices after the app is closed
    /// </summary>
    public bool ShouldRestoreDevices { get; set; } = false;

    public override string ToString()
    {
        return new TriggerFactory().Get(Type).Label;
    }

    public bool Equals(Trigger other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Type == other.Type && WindowName == other.WindowName && ApplicationPath == other.ApplicationPath && Equals(HotKey, other.HotKey);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Trigger)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (int)Type;
            hashCode = (hashCode * 397) ^ (WindowName != null ? WindowName.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (ApplicationPath != null ? ApplicationPath.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (HotKey != null ? HotKey.GetHashCode() : 0);
            return hashCode;
        }
    }

    public static bool operator ==(Trigger left, Trigger right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Trigger left, Trigger right)
    {
        return !Equals(left, right);
    }
}