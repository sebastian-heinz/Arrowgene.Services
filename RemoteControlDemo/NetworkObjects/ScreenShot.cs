using System;

namespace NetworkObjects
{
    [Serializable]
    public class ScreenShot
    {
        public ScreenShot(byte[] screen)
        {
            this.Screen = screen;
        }

        public byte[] Screen { get; set; }

    }
}
