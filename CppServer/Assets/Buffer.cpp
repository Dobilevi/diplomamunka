#include "Buffer.h"

#include <stdexcept>

void Buffer::SetBuffer(std::shared_ptr<char[]> buffer, uint16_t size) {
    this->buffer = buffer;
    this->size = size;
    index = 0;
}

std::shared_ptr<char[]> Buffer::GetBuffer() const { return buffer; }

uint16_t Buffer::GetBufferSize() const { return size; }

uint16_t Buffer::GetSize() const { return index; }

void Buffer::Reset(uint16_t startIndex) { index = startIndex; }

void Buffer::CheckSize(uint16_t length) const {
    if ((index + length) > size) {
        throw std::runtime_error("Buffer is not large enough!");
    }
}

void Buffer::WriteString(const std::u16string& value) {
    CheckSize(sizeof(char16_t) * value.length());

    std::memcpy(buffer.get() + index, value.c_str(),
                sizeof(char16_t) * value.length());
    index += sizeof(char16_t) * value.length();
}

void Buffer::ReadString(std::u16string& out, uint16_t length,
                        uint16_t maxLength) {
    length = std::min(length, maxLength);

    CheckSize(sizeof(char16_t) * length);

    std::unique_ptr<char16_t[]> value{new char16_t[length + 1]};
    const char16_t nul = 0;
    std::memcpy(value.get() + length, &nul, sizeof(char16_t));
    std::memcpy(value.get(), buffer.get() + index, sizeof(char16_t) * length);
    out = value.get();
    index += sizeof(char16_t) * length;
}
