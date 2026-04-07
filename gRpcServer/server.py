import grpc
import asyncio
import chat_pb2
import chat_pb2_grpc
from datetime import datetime

class ChatServiceServicer(chat_pb2_grpc.ChatServiceServicer):

    async def Chat(self, request_iterator, context):
        print("클라이언트 연결됨!")
        try:
            async for msg in request_iterator:
                print(f"받은 메시지: [{msg.user}]: {msg.message}")
                yield chat_pb2.ChatMessage(
                    user=msg.user,
                    message=msg.message,
                    timestamp=datetime.now().strftime("%H:%M:%S")
                )
        except Exception as e:
            print(f"에러 발생: {e}")


async def serve():
    server = grpc.aio.server()
    chat_pb2_grpc.add_ChatServiceServicer_to_server(ChatServiceServicer(), server)
    server.add_insecure_port("[::]:50051")
    await server.start()
    print("서버 시작! 포트: 50051")
    await server.wait_for_termination()

if __name__ == "__main__":
    asyncio.run(serve())
