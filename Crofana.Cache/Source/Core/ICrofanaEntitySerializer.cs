using System.IO;

namespace Crofana.Cache
{
    public interface ICrofanaEntitySerializer
    {
        void Deserialize(Stream stream, ICrofanaEntityManager target);
    }
}
