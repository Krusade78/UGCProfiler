#include "framework.h"
#include "CHIDOutput.h"
#include <thread>

CHIDOutput::CHIDOutput(CProfile& profile, CEventQueue& evQueue, CVirtualHID& vhid)
    : profile(profile), evQueue(evQueue), vhid(vhid)
{
    evExit.set(CreateEvent(NULL, TRUE, FALSE, NULL));
}

CHIDOutput::~CHIDOutput()
{
    SetEvent(evExit.get());
    if (threadClosed.valid())
    {
        WaitForSingleObject(threadClosed.get(), INFINITE);
    }
}

bool CHIDOutput::Init()
{
    output = std::make_unique<CProcessOutput>(profile, vhid);

    threadClosed.set(reinterpret_cast<HANDLE>(_beginthreadex(nullptr, 0, ThreadRead, this, 0, nullptr)));

    return threadClosed.valid();
}

unsigned _stdcall CHIDOutput::ThreadRead(void* param)
{
    auto* local = static_cast<CHIDOutput*>(param);

    SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL);

    HANDLE events[3]= { local->evExit.get(), local->evQueue.GetEvQueue(), local->output->GetEvQueue()};
    while (true)
    {

        DWORD ev = WaitForMultipleObjects(3, events, FALSE, INFINITE);
        if ((ev - WAIT_OBJECT_0) != 0)
        {
            CEventPacket* paq = nullptr;
            if ((ev - WAIT_OBJECT_0) == 1)
            {
                paq = local->evQueue.Read();
            }
            local->output->Process(paq);
        }
        else
        {
            break;
        }
    }

    return 0;
}
