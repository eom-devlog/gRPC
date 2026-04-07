import grpc
import chat_pb2
import chat_pb2_grpc
import asyncio

async def test():
    async with grpc.aio.insecure_channel('localhost:50051') as channel:
        stub = chat_pb2_grpc.ChatServiceStub(channel)
        async def gen():
            # 서버에 한 번만 보낼 메시지 (스트리밍 시작용)
            yield chat_pb2.ChatMessage(user="pytest", message="hello", timestamp="00:00:00")
            await asyncio.sleep(1)  # 응답 기다릴 시간
        # 서버에서 오는 응답을 비동기로 읽음
        async for response in stub.Chat(gen()):
            print("서버 응답:", response.message)

if __name__ == "__main__":
    asyncio.run(test())
