
# C++ Server

## Build

### Linux

```bash
cmake -Bbuild -H.
make -j4 -C build
chmod 777 build/CppServer
```

### Windows

```bash
mkdir build
cd build
cmake ..
cmake --build .
```

## clang-format

```bash
clang-format -i ./*.h ./*.cpp
clang-format -i ./Assets/*.h ./Assets/*.cpp
clang-format -i ./NamedPipes/*.h ./NamedPipes/*.cpp
clang-format -i ./UDP/*.h ./UDP/*.cpp
```
