using Grpc.Core;
using GrpcGreeter;
using System.Threading.Channels;

namespace GrpcGreeter.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
        public override async Task SayHelloStream(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            for (var i = 0; i < 100; i++) {
                await responseStream.WriteAsync(new HelloReply
                {
                    Message = $"Hello {i} {request.Name}"
                });
                await Task.Delay(500);
            }
        }
        public override async Task SayHelloBidirectionStream(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            // we'll use a channel here to handle in-process 'messages' concurrently being written to and read from the channel.
            var channel = Channel.CreateUnbounded<HelloRequest>();
            _ = Task.Run(async () =>
            {
                await foreach (var request in requestStream.ReadAllAsync())
                {
                    await responseStream.WriteAsync(new HelloReply
                    {
                        Message = $"Ditto {request.Name}"
                    });
                }
            });
        }


    }
}
