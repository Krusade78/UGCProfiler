#pragma once
#include <assert.h>
#include <mutex>

class unique_handle
{
public:
    unique_handle() noexcept = default;
    explicit unique_handle(HANDLE h) noexcept
    {
        set(h);
    }

    ~unique_handle() noexcept
    {
        HANDLE old = handle.exchange(nullptr);
        if (old)
        {
            CloseHandle(old);
        }
    }

    unique_handle(const unique_handle&) = delete;
    unique_handle& operator=(const unique_handle&) = delete;
    unique_handle(unique_handle&&) = delete;
    unique_handle& operator=(unique_handle&&) = delete;

    void set(HANDLE h) noexcept
    {
        assert(handle.load() == nullptr && "Assignment into non-empty unique_handle");
        if (handle.load() != nullptr)
        {
            std::terminate();
        }
        if (h == INVALID_HANDLE_VALUE)
        {
            return;
        }
        handle.exchange(h);
    }

    HANDLE move() noexcept
    {
        return handle.exchange(nullptr);
    }

    HANDLE get() const noexcept
    {
        return handle.load();
    }

    bool valid() const noexcept
    {
        return get() != nullptr;
    }

private:
    std::atomic<HANDLE> handle{ nullptr };
};
