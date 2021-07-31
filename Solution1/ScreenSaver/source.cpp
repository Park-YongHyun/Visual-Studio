#include <Windows.h>
#include <ScrnSave.h>

#ifdef UNICODE
	#pragma comment(lib, "ScrnSavW.lib")
#else
	#pragma comment(lib, "ScrnSave.lib")
#endif

#pragma comment(lib,"ComCtl32.lib")

/*
    스크린 세이버의 메시지를 처리하는 메시지 프로시저 
    원형도 WndProc과 동일하며 프로그래밍 방법도 동일 
    스크린 세이버 라이브러리는 애플릿에 적절한 전체화면의 검정색 배경에 항상 위 옵션으로 메인 윈도우를 만들며 
    메인 윈도우의 메시지 프로시저로 이 함수를 지정한다.
    이 함수에서 처리하지 않는 메시지는 DefScreenSaverProc함수로 전달하여 스크린 세이버 라이브러리가 처리 
*/
LRESULT WINAPI ScreenSaverProc(HWND hWnd, UINT iMessage, WPARAM wParam, LPARAM lParam)
{
    switch(iMessage){
    //case WM_CREATE:
    //    return 0;

    //case WM_PAINT:
    //    return 0;

	case WM_KEYDOWN:
		return 0;

	case WM_MOUSEMOVE:
		return 0;

	//case WM_DESTROY:
	//	return 0;
    }
    return DefScreenSaverProc(hWnd,iMessage,wParam,lParam);
}

/* 
    설정 대화상자의 메시지 처리 함수. 일반적인 대화상자 메시지 프로시저와 동일 
    사용자로부터 설정 상태를 입력받아 약속된 장소(보통 레지스트리)에 저장하는 역할 
*/
BOOL WINAPI ScreenSaverConfigureDialog(HWND hDlg, UINT iMessage, WPARAM wParam, LPARAM lParam)
{
    return TRUE;
}

/* 
    커스텀 컨트롤을 등록할 때 사용. 보통 TRUE를 리턴하면 된다.* 
*/
BOOL WINAPI RegisterDialogClasses(HANDLE hInst)
{
    return TRUE;
}