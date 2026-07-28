namespace ProperSave.Utils
{
    public class ObjectBuffer
    {
        public static readonly int maxLength = 5;
        public static readonly object[][] Buffers;

        static ObjectBuffer()
        {
            Buffers = new object[maxLength][];
            for (var i = 0; i < maxLength; i++)
            {
                Buffers[i] = new object[i];
            }
        }

        public static void Clear()
        {
            for (var i = 0; i < Buffers.Length; i++)
            {
                var buffer = Buffers[i];
                for (var j = 0; j < buffer.Length; j++)
                {
                    buffer[j] = null;
                }
            }
        }
    }
}
