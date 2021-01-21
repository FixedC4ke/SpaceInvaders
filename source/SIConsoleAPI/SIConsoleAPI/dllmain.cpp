// dllmain.cpp : Определяет точку входа для приложения DLL.
#include "pch.h"
#include <conio.h>
#include <windows.h>
#include <stdio.h>
#include <iostream>
#include <string>

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

    CHAR_INFO charInfo[9 * 3];

    COORD charBufSize = { 9, 3 };   
    COORD characterPos = { 0, 0 }; 
    SMALL_RECT writeArea = { x, y, x + 9, y + 3 };


    for (int i = 0; i < (9 * 3); i++)
    {
        charInfo[i].Char.AsciiChar = ' ';
        charInfo[i].Attributes = clean?NULL:BACKGROUND_GREEN;
    }

    WriteConsoleOutputA(wHnd, charInfo, charBufSize, characterPos, &writeArea);


    return 0;
}

extern "C" int _stdcall DrawShip(short x, short y, bool clean)
{
    HANDLE wHnd = GetStdHandle(STD_OUTPUT_HANDLE);

    CHAR_INFO charInfo[6 * 2];

    COORD charBufSize = { 6, 2 };
    COORD characterPos = { 0, 0 };
    SMALL_RECT writeArea = { x, y, x + 6, y + 2 };


    for (int i = 0; i < (6 * 2); i++)
    {
        charInfo[i].Char.AsciiChar = ' ';
        charInfo[i].Attributes = clean ? NULL : BACKGROUND_GREEN|BACKGROUND_BLUE|BACKGROUND_RED;
    }
    charInfo[2].Attributes = NULL;
    charInfo[3].Attributes = NULL;

    charInfo[7].Attributes = NULL;
    charInfo[10].Attributes = NULL;

    WriteConsoleOutputA(wHnd, charInfo, charBufSize, characterPos, &writeArea);

    return 0;
}

extern "C" int _stdcall DrawPatron(short x, short y, bool clean)
{
    HANDLE wHnd = GetStdHandle(STD_OUTPUT_HANDLE);

    CHAR_INFO charInfo[1 * 2];

    COORD charBufSize = { 1, 2 };
    COORD characterPos = { 0, 0 };
    SMALL_RECT writeArea = { x, y, x + 1, y + 2 };


    for (int i = 0; i < (1 * 2); i++)
    {
        charInfo[i].Char.AsciiChar = ' ';
        charInfo[i].Attributes = clean ? NULL : BACKGROUND_RED;
    }

    WriteConsoleOutputA(wHnd, charInfo, charBufSize, characterPos, &writeArea);
    return 0;
}

extern "C" int _stdcall DrawBomb(short x, short y, bool clean)
{
    HANDLE wHnd = GetStdHandle(STD_OUTPUT_HANDLE);

    CHAR_INFO charInfo[1 * 2];

    COORD charBufSize = { 1, 2 };
    COORD characterPos = { 0, 0 };
    SMALL_RECT writeArea = { x, y, x + 1, y + 2 };


    for (int i = 0; i < (1 * 2); i++)
    {
        charInfo[i].Char.AsciiChar = ' ';
        charInfo[i].Attributes = clean ? NULL : BACKGROUND_RED|BACKGROUND_GREEN;
    }

    WriteConsoleOutputA(wHnd, charInfo, charBufSize, characterPos, &writeArea);
    return 0;
}

extern "C" int _stdcall ShowHP(short x) {
    HANDLE wHnd = GetStdHandle(STD_OUTPUT_HANDLE);

    COORD curspos = { 2, 40 };
    DWORD written = 0;
    string message = "Lives: " + to_string(x);
    SetConsoleCursorPosition(wHnd, curspos);
    WriteConsoleA(wHnd, message.c_str(), strlen(message.c_str()), &written, NULL);
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
