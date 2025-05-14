#ifndef BUFFERWRITER_H
#define BUFFERWRITER_H

#include <cstdint>
#include <cstring>
#include <string>

#include "MessageType.h"

class Buffer {
    uint16_t size;
    char* buffer = nullptr;
    uint16_t index = 0;

    void CheckSize(uint16_t length) const;

   public:
    Buffer(uint16_t size = 256);

    ~Buffer();

    void SetBuffer(char* buffer, uint16_t size);

    char* GetBuffer() const;

    uint16_t GetBufferSize() const;

    uint16_t GetSize() const;

    void Reset(uint16_t startIndex = 0);

    template <typename T>
    void Write(T value) {
        CheckSize(sizeof(T));

        std::memcpy(buffer + index, &value, sizeof(T));
        index += sizeof(T);
    }

    void WriteString(const std::u16string& value);

    void ReadString(std::u16string& out, uint16_t length, uint16_t maxLength);

    template <typename T>
    void Read(T& out) {
        CheckSize(sizeof(T));

        std::memcpy(&out, buffer + index, sizeof(T));
        index += sizeof(T);
    }
};

#endif  // BUFFERWRITER_H
