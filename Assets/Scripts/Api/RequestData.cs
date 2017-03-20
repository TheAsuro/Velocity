using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Api
{
    public interface RequestData
    {
        void WriteData(Stream stream);
        string GetContentType();
    }

    public class BinaryRequestData : RequestData
    {
        private byte[] data;

        public BinaryRequestData(byte[] data)
        {
            this.data = data;
        }

        public void WriteData(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(data);
                writer.Flush();
            }
        }

        public string GetContentType()
        {
            return "application/octet-stream";
        }
    }

    public class StringRequestData : Dictionary<string, string>, RequestData
    {
        public void WriteData(Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding.ASCII))
            {
                UseStreamWriter(writer);
            }
        }

        public string GetContentType()
        {
            return "application/x-www-form-urlencoded";
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            using (StringWriter writer = new StringWriter(builder))
            {
                UseStreamWriter(writer);
            }
            return builder.ToString();
        }

        private void UseStreamWriter(TextWriter writer)
        {
            bool first = true;

            foreach (KeyValuePair<string, string> pair in this)
            {
                if (!first)
                    writer.Write("&");
                else
                    first = false;

                writer.Write(pair.Key + "=" + pair.Value);
            }

            writer.Flush();
        }
    }
}