namespace RawInputHelper
{
    public class RawInput(RawInput.WndProcCallback callback)
    {
        public delegate void WndProcCallback(string hidInterface, byte[] hidData);

        private URawInput? unmanaged = new();

        ~RawInput()
        {
            Close();
        }

        public void Init()
        {
            unmanaged?.Init(this);
        }

        public void Close()
        {
            if (unmanaged != null)
            {
                unmanaged.Close();
                unmanaged = null;
            }
        }


        public void Call(string hidInterface, byte[] data)
        {
            callback.Invoke(hidInterface, data);
        }
    }
}
