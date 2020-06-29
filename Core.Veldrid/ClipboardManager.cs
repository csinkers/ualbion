using System;
using System.Runtime.InteropServices;
using UAlbion.Core.Events;

namespace UAlbion.Core.Veldrid
{
    public class ClipboardManager : Component
    {
        public ClipboardManager()
        {
            On<SetClipboardTextEvent>(SetText);
        }

#if Windows
        void SetText(SetClipboardTextEvent e)
        {
            /* Throwing access violations :/
            Thread thread = new Thread(() =>
            {
                if (OpenClipboard(IntPtr.Zero))
                {
                    var ptr = Marshal.StringToHGlobalUni(e.Text);
                    SetClipboardData(13, ptr);
                    CloseClipboard();
                    Marshal.FreeHGlobal(ptr);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            */
        }

        [DllImport("user32.dll")]
        static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        static extern bool SetClipboardData(uint uFormat, IntPtr data);

#elif Linux
        void SetText(SetClipboardTextEvent e) { // TODO }
#elif OSX
        void SetText(SetClipboardTextEvent e) { // TODO }
#else
        void SetText(SetClipboardTextEvent e) { // TODO }
#endif
    }
}
