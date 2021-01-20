// dllmain.cpp : Определяет точку входа для приложения DLL.
#include "pch.h"
#include <conio.h>
#include <windows.h>
#include <stdio.h>
#include <iostream>

using namespace std;


BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

extern "C" int _stdcall DrawCart(short x, short y, bool clean)
{
    HANDLE wHnd = GetStdHandle(STD_OUTPUT_HANDLE);
    

    SMALL_RECT srctWriteRect = { x, y, x+9, x+5 };
    COORD crd = { 0, 0 };
    COORD bsize = { 9, 3 };

    CHAR_INFO chInfo[9 * 3];
    for (size_t i = 0; i < 9 * 3; ++i)
    {
        chInfo[i].Char.AsciiChar = clean?NULL:' ';
        chInfo[i].Attributes = clean?7:BACKGROUND_GREEN;
    }

    WriteConsoleOutputA(wHnd, chInfo, bsize, crd, &srctWriteRect);
    return 0;
}

extern "C" int _stdcall DrawShip(short x, short y, bool clean)
{
    HANDLE wHnd = GetStdHandle(STD_OUTPUT_HANDLE);

    SMALL_RECT srctWriteRect = { x, y, x + 6, x + 2 };
    COORD crd = { 0, 0 };
    COORD bsize = { 6, 2 };

    CHAR_INFO chInfo[6 * 2];
    for (size_t i = 0; i < 6 * 2; ++i)
    {
        chInfo[i].Char.AsciiChar = clean ? NULL : ' ';
        chInfo[i].Attributes = clean ? 7 : BACKGROUND_BLUE | BACKGROUND_GREEN | BACKGROUND_RED;
    }

    WriteConsoleOutputA(wHnd, chInfo, bsize, crd, &srctWriteRect);
    return 0;
}

extern "C" int _stdcall DrawPatron(short x, short y, bool clean)
{
    HANDLE wHnd = GetStdHandle(STD_OUTPUT_HANDLE);

    SMALL_RECT srctWriteRect = { x, y, x + 1, x + 2 };
    COORD crd = { 0, 0 };
    COORD bsize = { 1, 2 };

    CHAR_INFO chInfo[1 * 2];
    for (size_t i = 0; i < 1 * 2; ++i)
    {
        chInfo[i].Char.AsciiChar = clean ? NULL : ' ';
        chInfo[i].Attributes = clean ? 7 : BACKGROUND_RED;
    }

    WriteConsoleOutputA(wHnd, chInfo, bsize, crd, &srctWriteRect);
    return 0;
}

extern "C" int _stdcall DrawBomb(short x, short y, bool clean)
{
    HANDLE wHnd = GetStdHandle(STD_OUTPUT_HANDLE);

    SMALL_RECT srctWriteRect = { x, y, x + 1, x + 2 };
    COORD crd = { 0, 0 };
    COORD bsize = { 1, 2 };

    CHAR_INFO chInfo[1 * 2];
    for (size_t i = 0; i < 1 * 2; ++i)
    {
        chInfo[i].Char.AsciiChar = clean ? NULL : ' ';
        chInfo[i].Attributes = clean ? 7 : BACKGROUND_RED|BACKGROUND_GREEN;
    }

    WriteConsoleOutputA(wHnd, chInfo, bsize, crd, &srctWriteRect);
    return 0;
}

extern "C" int _stdcall InitializeConsole() {
    HANDLE wHnd = GetStdHandle(STD_OUTPUT_HANDLE);

    SMALL_RECT pos = { 0, 0, 109, 40 };
    COORD bufsize = { 110, 41 };
    CONSOLE_CURSOR_INFO cinfo;
    GetConsoleCursorInfo(wHnd, &cinfo);
    cinfo.bVisible = false;
    SetConsoleWindowInfo(wHnd, true, &pos);
    SetConsoleScreenBufferSize(wHnd, bufsize);
    SetConsoleCursorInfo(wHnd, &cinfo);
    SetConsoleTitleA("Space Invaders");
    return 0;
}
