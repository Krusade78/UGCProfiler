#include "framework.h"
#include "CHIDOutput.h"

CHIDOutput::CHIDOutput(CProfile* profile, CEventQueue* evQueue, CVirtualHID* vhid)
{
    this->profile = profile;
    this->evQueue = evQueue;
    this->vhid = vhid;
    evExit = CreateEvent(NULL, TRUE, FALSE, NULL);
}

CHIDOutput::~CHIDOutput()
{
    exit = true;
    SetEvent(evExit);
    while (InterlockedCompareExchange16(&threadClosed, 0, 0) == FALSE) Sleep(500);
    CloseHandle(evExit);
    if (output != nullptr) delete output;
}

bool CHIDOutput::Init()
{
    output = new CProcessOutput(profile, vhid);

    HANDLE hilo = CreateThread(NULL, 0, ThreadRead, this, 0, NULL);
    if (hilo != NULL)
    {
        while (InterlockedCompareExchange16(&threadClosed, FALSE, FALSE))
        {
            Sleep(500);
        }
        return true;
    }

	return false;
}

DWORD WINAPI CHIDOutput::ThreadRead(LPVOID param)
{
    CHIDOutput* local = (CHIDOutput*)param;
    InterlockedExchange16(&local->threadClosed, FALSE);

    SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL);

    HANDLE events[3]= { local->evExit, local->evQueue->GetEvQueue(), local->output->GetEvQueue()};
    while (!local->exit)
    {

        DWORD ev = WaitForMultipleObjects(3, events, FALSE, INFINITE);
        if ((ev - WAIT_OBJECT_0) != 0)
        {
            CEventPacket* paq = nullptr;
            if ((ev - WAIT_OBJECT_0) == 1)
            {
                paq = local->evQueue->Read();
            }
            local->output->Process(paq);
        }
    }

    InterlockedExchange16(&local->threadClosed, TRUE);
    return 0;
}
