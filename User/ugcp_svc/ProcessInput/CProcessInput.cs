using System;
using System.Runtime.InteropServices;

namespace ugcp_svc.ProcessInput
{
    sealed class CProcessInput : IDisposable
    {
        private readonly Profile.CProfile pProfile;
        private HIDInput.HID_INPUT_DATA lastStatus = new();
        private readonly GenerateEvents.CButtonsHats? genBtHat;

        public CProcessInput(Profile.CProfile refProfile)
        {
            pProfile = refProfile;
            genBtHat = GenerateEvents.CButtonsHats.GetInstance(refProfile);
        }

        private byte disposed;
        public void Dispose()
        {
            if (System.Threading.Interlocked.CompareExchange(ref disposed, 1, 0) == 1) return;

            genBtHat?.Dispose();
        }


        private void GetOldHidData(ref HIDInput.HID_INPUT_DATA data)
        {
            Span<byte> srcBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref lastStatus, 1));
            Span<byte> dstBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref data, 1));
            srcBytes.CopyTo(dstBytes);
        }

        private void SetOldHidData(HIDInput.HID_INPUT_DATA data)
        {
            Span<byte> srcBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref data, 1));
            Span<byte> dstBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref lastStatus, 1));
            srcBytes.CopyTo(dstBytes);
        }

        public void Process(uint joyId, HIDInput.HID_INPUT_DATA p_hidData)
        {
            HIDInput.HID_INPUT_DATA viejohidData = default;

            GetOldHidData(ref viejohidData);
            SetOldHidData(p_hidData);

            {
                Span<byte> spanA = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref viejohidData, 1));
                Span<byte> spanB = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref p_hidData, 1));
                if (spanA.SequenceCompareTo(spanB) == 0)
                {
                    return;
                }
            }

            if (!pProfile.RawMode /*&& !pProfile.CalibrationMode*/)
            {
                //	Buttons

                for (byte idx = 0; idx < 2; idx++)
                {
                    ulong changed = p_hidData.Buttons[idx] ^ viejohidData.Buttons[idx];
                    if (changed != 0)
                    {
                        for (byte exp = 0; exp < 64; exp++)
                        {
                            if (((changed >> exp) & 1) == 1)
                            { // if button has changed
                                if (((p_hidData.Buttons[idx] >> exp) & 1) == 1)
                                    genBtHat?.PressButton(joyId, (byte)((idx * 64) + exp), false);
                                else
                                    genBtHat?.ReleaseButton(joyId, (byte)((idx * 64) + exp), false);
                            }
                        }
                    }
                }

                // Hats

                for (byte idx = 0; idx < 4; idx++)
                {
                    if (p_hidData.Hats[idx] != viejohidData.Hats[idx])
                    {
                        if (viejohidData.Hats[idx] != 255)
                            genBtHat?.ReleaseHat(joyId, (byte)((idx * 8) + viejohidData.Hats[idx]), false);
                        if (p_hidData.Hats[idx] != 255)
                            genBtHat?.PressHat(joyId, (byte)((idx * 8) + p_hidData.Hats[idx]), false);
                    }
                }

                // Axes
                for (byte idx = 0; idx < 24; idx++)
                {
                    if (p_hidData.Axis[idx] != viejohidData.Axis[idx])
                        GenerateEvents.CAxes.MoveAxis(pProfile, joyId, idx, p_hidData.Axis[idx]);
                }

                // Sensibility and mapping
                GenerateEvents.CAxes.SensibilityAndMapping(pProfile, joyId, ref viejohidData, ref p_hidData);
            }
        }
    }
}
