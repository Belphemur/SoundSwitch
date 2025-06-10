using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SoundSwitch.UI.Component.ListView;

public class ListViewExtended : System.Windows.Forms.ListView
{
    private const int LVM_FIRST = 0x1000;                    // ListView messages
    private const int LVM_SETGROUPINFO = (LVM_FIRST + 147);  // ListView messages Setinfo on Group
    private const int WM_LBUTTONUP = 0x0202;                 // Windows message left button

    private delegate void CallBackSetGroupState(ListViewGroup lstvwgrp, ListViewGroupState state);
    private delegate void CallbackSetGroupString(ListViewGroup lstvwgrp, string value);

    /// <summary>
    /// Sends the specified message to a window or windows. The SendMessage function calls the window procedure for the specified window and does not return until the window procedure has processed the message. 
    /// To send a message and return immediately, use the SendMessageCallback or SendNotifyMessage function. To post a message to a thread's message queue and return immediately, use the PostMessage or PostThreadMessage function.
    /// </summary>
    /// <param name="hWnd">
    /// [in] Handle to the window whose window procedure will receive the message. 
    /// If this parameter is HWND_BROADCAST, the message is sent to all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but the message is not sent to child windows. 
    /// Microsoft Windows Vista and later. Message sending is subject to User Interface Privilege Isolation (UIPI). The thread of a process can send messages only to message queues of threads in processes of lesser or equal integrity level.
    /// </param>
    /// <param name="uMsg">[in] Specifies the message to be sent.</param>
    /// <param name="wParam">[in] Specifies additional message-specific information.</param>
    /// <param name="lParam">[in] Type of LVGROUP, Specifies additional message-specific information.</param>
    /// <returns>
    /// Microsoft Windows Vista and later. When a message is blocked by UIPI the last error, retrieved with GetLastError, is set to 5 (access denied).
    /// Applications that need to communicate using HWND_BROADCAST should use the RegisterWindowMessage function to obtain a unique message for inter-application communication.
    /// The system only does marshalling for system messages (those in the range 0 to (WM_USER-1)). To send other messages (those >= WM_USER) to another process, you must do custom marshalling.
    /// If the specified window was created by the calling thread, the window procedure is called immediately as a subroutine. If the specified window was created by a different thread, the system switches to that thread and calls the appropriate window procedure. Messages sent between threads are processed only when the receiving thread executes message retrieval code. The sending thread is blocked until the receiving thread processes the message. However, the sending thread will process incoming nonqueued messages while waiting for its message to be processed. To prevent this, use SendMessageTimeout with SMTO_BLOCK set. For more information on nonqueued messages, see Nonqueued Messages.
    /// Windows 95/98/Me: SendMessageW is supported by the Microsoft Layer for Unicode (MSLU). To use this, you must add certain files to your application, as outlined in Microsoft Layer for Unicode on Windows 95/98/Me Systems.
    /// </returns>
    [DllImport("User32.dll"), Description("Sends the specified message to a window or windows. The SendMessage function calls the window procedure for the specified window and does not return until the window procedure has processed the message. To send a message and return immediately, use the SendMessageCallback or SendNotifyMessage function. To post a message to a thread's message queue and return immediately, use the PostMessage or PostThreadMessage function.")]
    private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, LVGROUP lParam);

    private static int? GetGroupID(ListViewGroup lstvwgrp)
    {
        int? rtnval = null;
        Type GrpTp = lstvwgrp.GetType();
        if (GrpTp != null)
        {
            PropertyInfo pi = GrpTp.GetProperty("ID", BindingFlags.NonPublic | BindingFlags.Instance);
            if (pi != null)
            {
                object tmprtnval = pi.GetValue(lstvwgrp, null);
                if (tmprtnval != null)
                {
                    rtnval = tmprtnval as int?;
                }
            }
        }
        return rtnval;
    }

    private static void SetGrpState(ListViewGroup lstvwgrp, ListViewGroupState state)
    {
        if (Environment.OSVersion.Version.Major < 6)   //Only Vista and forward allows collaps of ListViewGroups
            return;
        if (lstvwgrp == null || lstvwgrp.ListView == null)
            return;
        if (lstvwgrp.ListView.InvokeRequired)
            lstvwgrp.ListView.Invoke(new CallBackSetGroupState(SetGrpState), lstvwgrp, state);
        else
        {
            int? GrpId = GetGroupID(lstvwgrp);
            int gIndex = lstvwgrp.ListView.Groups.IndexOf(lstvwgrp);
            LVGROUP group = new LVGROUP();
            group.CbSize = Marshal.SizeOf(group);
            group.State = state;
            group.Mask = ListViewGroupMask.State;
            if (GrpId != null)
            {
                group.IGroupId = GrpId.Value;
                SendMessage(lstvwgrp.ListView.Handle, LVM_SETGROUPINFO, GrpId.Value, group);
            }
            else
            {
                group.IGroupId = gIndex;
                SendMessage(lstvwgrp.ListView.Handle, LVM_SETGROUPINFO, gIndex, group);
            }
            lstvwgrp.ListView.Refresh();
        }
    }

    private static void setGrpFooter(ListViewGroup lstvwgrp, string footer)
    {
        if (Environment.OSVersion.Version.Major < 6)   //Only Vista and forward allows footer on ListViewGroups
            return;
        if (lstvwgrp == null || lstvwgrp.ListView == null)
            return;
        if (lstvwgrp.ListView.InvokeRequired)
            lstvwgrp.ListView.Invoke(new CallbackSetGroupString(setGrpFooter), lstvwgrp, footer);
        else
        {
            int? GrpId = GetGroupID(lstvwgrp);
            int gIndex = lstvwgrp.ListView.Groups.IndexOf(lstvwgrp);
            LVGROUP group = new LVGROUP();
            group.CbSize = Marshal.SizeOf(group);
            group.PszFooter = footer;
            group.Mask = ListViewGroupMask.Footer;
            if (GrpId != null)
            {
                group.IGroupId = GrpId.Value;
                SendMessage(lstvwgrp.ListView.Handle, LVM_SETGROUPINFO, GrpId.Value, group);
            }
            else
            {
                group.IGroupId = gIndex;
                SendMessage(lstvwgrp.ListView.Handle, LVM_SETGROUPINFO, gIndex, group);
            }
        }
    }

    public void SetGroupsState(ListViewGroupState state)
    {
        foreach (ListViewGroup lvg in Groups)
            SetGrpState(lvg, state);
    }

    public void SetGroupState(string name, ListViewGroupState state)
    {
        var group = Groups[name];
        if (group == null)
        {
            throw new ArgumentOutOfRangeException($"Unknown group {name}");
        }

        SetGrpState(group, state);
    }

    public void SetGroupFooter(ListViewGroup lvg, string footerText)
    {
        setGrpFooter(lvg, footerText);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_LBUTTONUP)
            base.DefWndProc(ref m);
        base.WndProc(ref m);
    }
}


/// <summary>
/// LVGROUP StructureUsed to set and retrieve groups.
/// </summary>
/// <example>
/// LVGROUP myLVGROUP = new LVGROUP();
/// myLVGROUP.CbSize	// is of managed type uint
/// myLVGROUP.Mask	// is of managed type uint
/// myLVGROUP.PszHeader	// is of managed type string
/// myLVGROUP.CchHeader	// is of managed type int
/// myLVGROUP.PszFooter	// is of managed type string
/// myLVGROUP.CchFooter	// is of managed type int
/// myLVGROUP.IGroupId	// is of managed type int
/// myLVGROUP.StateMask	// is of managed type uint
/// myLVGROUP.State	// is of managed type uint
/// myLVGROUP.UAlign	// is of managed type uint
/// myLVGROUP.PszSubtitle	// is of managed type IntPtr
/// myLVGROUP.CchSubtitle	// is of managed type uint
/// myLVGROUP.PszTask	// is of managed type string
/// myLVGROUP.CchTask	// is of managed type uint
/// myLVGROUP.PszDescriptionTop	// is of managed type string
/// myLVGROUP.CchDescriptionTop	// is of managed type uint
/// myLVGROUP.PszDescriptionBottom	// is of managed type string
/// myLVGROUP.CchDescriptionBottom	// is of managed type uint
/// myLVGROUP.ITitleImage	// is of managed type int
/// myLVGROUP.IExtendedImage	// is of managed type int
/// myLVGROUP.IFirstItem	// is of managed type int
/// myLVGROUP.CItems	// is of managed type IntPtr
/// myLVGROUP.PszSubsetTitle	// is of managed type IntPtr
/// myLVGROUP.CchSubsetTitle	// is of managed type IntPtr
/// </example>
/// <remarks>
/// The LVGROUP structure was created by Paw Jershauge
/// Created: Jan. 2008.
/// The LVGROUP structure code is based on information from Microsoft's MSDN2 website.
/// The structure is generated via an automated converter and is as is.
/// The structure may or may not hold errors inside the code, so use at own risk.
/// Reference url: http://msdn.microsoft.com/en-us/library/bb774769(VS.85).aspx
/// </remarks>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode), Description("LVGROUP StructureUsed to set and retrieve groups.")]
public struct LVGROUP
{
    /// <summary>
    /// Size of this structure, in bytes.
    /// </summary>
    [Description("Size of this structure, in bytes.")]
    public int CbSize;

    /// <summary>
    /// Mask that specifies which members of the structure are valid input. One or more of the following values:LVGF_NONENo other items are valid.
    /// </summary>
    [Description("Mask that specifies which members of the structure are valid input. One or more of the following values:LVGF_NONE No other items are valid.")]
    public ListViewGroupMask Mask;

    /// <summary>
    /// Pointer to a null-terminated string that contains the header text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the header text.
    /// </summary>
    [Description("Pointer to a null-terminated string that contains the header text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the header text.")]
    [MarshalAs(UnmanagedType.LPWStr)]
    public string PszHeader;

    /// <summary>
    /// Size in TCHARs of the buffer pointed to by the pszHeader member. If the structure is not receiving information about a group, this member is ignored.
    /// </summary>
    [Description("Size in TCHARs of the buffer pointed to by the pszHeader member. If the structure is not receiving information about a group, this member is ignored.")]
    public int CchHeader;

    /// <summary>
    /// Pointer to a null-terminated string that contains the footer text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the footer text.
    /// </summary>
    [Description("Pointer to a null-terminated string that contains the footer text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the footer text.")]
    [MarshalAs(UnmanagedType.LPWStr)]
    public string PszFooter;

    /// <summary>
    /// Size in TCHARs of the buffer pointed to by the pszFooter member. If the structure is not receiving information about a group, this member is ignored.
    /// </summary>
    [Description("Size in TCHARs of the buffer pointed to by the pszFooter member. If the structure is not receiving information about a group, this member is ignored.")]
    public int CchFooter;

    /// <summary>
    /// ID of the group.
    /// </summary>
    [Description("ID of the group.")]
    public int IGroupId;

    /// <summary>
    /// Mask used with LVM_GETGROUPINFO (Microsoft Windows XP and Windows Vista) and LVM_SETGROUPINFO (Windows Vista only) to specify which flags in the state value are being retrieved or set.
    /// </summary>
    [Description("Mask used with LVM_GETGROUPINFO (Microsoft Windows XP and Windows Vista) and LVM_SETGROUPINFO (Windows Vista only) to specify which flags in the state value are being retrieved or set.")]
    public int StateMask;

    /// <summary>
    /// Flag that can have one of the following values:LVGS_NORMALGroups are expanded, the group name is displayed, and all items in the group are displayed.
    /// </summary>
    [Description("Flag that can have one of the following values:LVGS_NORMAL Groups are expanded, the group name is displayed, and all items in the group are displayed.")]
    public ListViewGroupState State;

    /// <summary>
    /// Indicates the alignment of the header or footer text for the group. It can have one or more of the following values. Use one of the header flags. Footer flags are optional. Windows XP: Footer flags are reserved.LVGA_FOOTER_CENTERReserved.
    /// </summary>
    [Description("Indicates the alignment of the header or footer text for the group. It can have one or more of the following values. Use one of the header flags. Footer flags are optional. Windows XP: Footer flags are reserved.LVGA_FOOTER_CENTERReserved.")]
    public uint UAlign;

    /// <summary>
    /// Windows Vista. Pointer to a null-terminated string that contains the subtitle text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the subtitle text. This element is drawn under the header text.
    /// </summary>
    [Description("Windows Vista. Pointer to a null-terminated string that contains the subtitle text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the subtitle text. This element is drawn under the header text.")]
    public IntPtr PszSubtitle;

    /// <summary>
    /// Windows Vista. Size, in TCHARs, of the buffer pointed to by the pszSubtitle member. If the structure is not receiving information about a group, this member is ignored.
    /// </summary>
    [Description("Windows Vista. Size, in TCHARs, of the buffer pointed to by the pszSubtitle member. If the structure is not receiving information about a group, this member is ignored.")]
    public uint CchSubtitle;

    /// <summary>
    /// Windows Vista. Pointer to a null-terminated string that contains the text for a task link when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the task text. This item is drawn right-aligned opposite the header text. When clicked by the user, the task link generates an LVN_LINKCLICK notification.
    /// </summary>
    [Description("Windows Vista. Pointer to a null-terminated string that contains the text for a task link when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the task text. This item is drawn right-aligned opposite the header text. When clicked by the user, the task link generates an LVN_LINKCLICK notification.")]
    [MarshalAs(UnmanagedType.LPWStr)]
    public string PszTask;

    /// <summary>
    /// Windows Vista. Size in TCHARs of the buffer pointed to by the pszTask member. If the structure is not receiving information about a group, this member is ignored.
    /// </summary>
    [Description("Windows Vista. Size in TCHARs of the buffer pointed to by the pszTask member. If the structure is not receiving information about a group, this member is ignored.")]
    public uint CchTask;

    /// <summary>
    /// Windows Vista. Pointer to a null-terminated string that contains the top description text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the top description text. This item is drawn opposite the title image when there is a title image, no extended image, and uAlign==LVGA_HEADER_CENTER.
    /// </summary>
    [Description("Windows Vista. Pointer to a null-terminated string that contains the top description text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the top description text. This item is drawn opposite the title image when there is a title image, no extended image, and uAlign==LVGA_HEADER_CENTER.")]
    [MarshalAs(UnmanagedType.LPWStr)]
    public string PszDescriptionTop;

    /// <summary>
    /// Windows Vista. Size in TCHARs of the buffer pointed to by the pszDescriptionTop member. If the structure is not receiving information about a group, this member is ignored.
    /// </summary>
    [Description("Windows Vista. Size in TCHARs of the buffer pointed to by the pszDescriptionTop member. If the structure is not receiving information about a group, this member is ignored.")]
    public uint CchDescriptionTop;

    /// <summary>
    /// Windows Vista. Pointer to a null-terminated string that contains the bottom description text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the bottom description text. This item is drawn under the top description text when there is a title image, no extended image, and uAlign==LVGA_HEADER_CENTER.
    /// </summary>
    [Description("Windows Vista. Pointer to a null-terminated string that contains the bottom description text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the bottom description text. This item is drawn under the top description text when there is a title image, no extended image, and uAlign==LVGA_HEADER_CENTER.")]
    [MarshalAs(UnmanagedType.LPWStr)]
    public string PszDescriptionBottom;

    /// <summary>
    /// Windows Vista. Size in TCHARs of the buffer pointed to by the pszDescriptionBottom member. If the structure is not receiving information about a group, this member is ignored.
    /// </summary>
    [Description("Windows Vista. Size in TCHARs of the buffer pointed to by the pszDescriptionBottom member. If the structure is not receiving information about a group, this member is ignored.")]
    public uint CchDescriptionBottom;

    /// <summary>
    /// Windows Vista. Index of the title image in the control imagelist.
    /// </summary>
    [Description("Windows Vista. Index of the title image in the control imagelist.")]
    public int ITitleImage;

    /// <summary>
    /// Windows Vista. Index of the extended image in the control imagelist.
    /// </summary>
    [Description("Windows Vista. Index of the extended image in the control imagelist.")]
    public int IExtendedImage;

    /// <summary>
    /// Windows Vista. Read-only.
    /// </summary>
    [Description("Windows Vista. Read-only.")]
    public int IFirstItem;

    /// <summary>
    /// Windows Vista. Read-only in non-owner data mode.
    /// </summary>
    [Description("Windows Vista. Read-only in non-owner data mode.")]
    public IntPtr CItems;

    /// <summary>
    /// Windows Vista. NULL if group is not a subset. Pointer to a null-terminated string that contains the subset title text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the subset title text.
    /// </summary>
    [Description("Windows Vista. NULL if group is not a subset. Pointer to a null-terminated string that contains the subset title text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the subset title text.")]
    public IntPtr PszSubsetTitle;

    /// <summary>
    /// Windows Vista. Size in TCHARs of the buffer pointed to by the pszSubsetTitle member. If the structure is not receiving information about a group, this member is ignored.
    /// </summary>
    [Description("Windows Vista. Size in TCHARs of the buffer pointed to by the pszSubsetTitle member. If the structure is not receiving information about a group, this member is ignored.")]
    public IntPtr CchSubsetTitle;
}

public enum ListViewGroupMask
{
    None = 0x00000,
    Header = 0x00001,
    Footer = 0x00002,
    State = 0x00004,
    Align = 0x00008,
    GroupId = 0x00010,
    SubTitle = 0x00100,
    Task = 0x00200,
    DescriptionTop = 0x00400,
    DescriptionBottom = 0x00800,
    TitleImage = 0x01000,
    ExtendedImage = 0x02000,
    Items = 0x04000,
    Subset = 0x08000,
    SubsetItems = 0x10000
}

[Flags]
public enum ListViewGroupState
{
    /// <summary>
    /// Groups are expanded, the group name is displayed, and all items in the group are displayed.
    /// </summary>
    Normal = 0,
    /// <summary>
    /// The group is collapsed.
    /// </summary>
    Collapsed = 1,
    /// <summary>
    /// The group is hidden.
    /// </summary>
    Hidden = 2,
    /// <summary>
    /// Version 6.00 and Windows Vista. The group does not display a header.
    /// </summary>
    NoHeader = 4,
    /// <summary>
    /// Version 6.00 and Windows Vista. The group can be collapsed.
    /// </summary>
    Collapsible = 8,
    /// <summary>
    /// Version 6.00 and Windows Vista. The group has keyboard focus.
    /// </summary>
    Focused = 16,
    /// <summary>
    /// Version 6.00 and Windows Vista. The group is selected.
    /// </summary>
    Selected = 32,
    /// <summary>
    /// Version 6.00 and Windows Vista. The group displays only a portion of its items.
    /// </summary>
    SubSeted = 64,
    /// <summary>
    /// Version 6.00 and Windows Vista. The subset link of the group has keyboard focus.
    /// </summary>
    SubSetLinkFocused = 128,


}