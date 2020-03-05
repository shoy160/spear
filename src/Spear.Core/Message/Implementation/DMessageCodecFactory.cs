namespace Spear.Core.Message.Implementation
{
    public class DMessageCodecFactory<TCodec> : IMessageCodecFactory
        where TCodec : IMessageCodec
    {
        private readonly TCodec _codec;

        public DMessageCodecFactory(TCodec codec)
        {
            _codec = codec;
        }

        public IMessageEncoder GetEncoder()
        {
            return _codec;
        }

        public IMessageDecoder GetDecoder()
        {
            return _codec;
        }
    }

    public class DMessageCodecFactory<TEncoder, TDecoder> : IMessageCodecFactory
        where TEncoder : IMessageEncoder, new()
        where TDecoder : IMessageDecoder, new()
    {
        private readonly TEncoder _encoder;
        private readonly TDecoder _decoder;

        public DMessageCodecFactory()
        {
            _encoder = new TEncoder();
            _decoder = new TDecoder();
        }

        public IMessageEncoder GetEncoder()
        {
            return _encoder;
        }

        public IMessageDecoder GetDecoder()
        {
            return _decoder;
        }
    }
}
