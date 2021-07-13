from Checker import Checker
import sys
import asyncio

async def Main():
    checker=Checker();
    checker.Load(sys.argv);
    await checker.Update();

if __name__ == '__main__':
    mainLoop = asyncio.get_event_loop();
    try:
        print("CheckFenix V2.0 Telegram bot");
        task_object_loop = mainLoop.create_task(Main());
        mainLoop.run_until_complete(task_object_loop);
    finally:
        mainLoop.close();



