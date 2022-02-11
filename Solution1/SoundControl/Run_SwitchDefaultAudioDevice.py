import os, win32api, win32con

if __name__ == '__main__':
	hwnd = ''

	with open(os.path.join(os.path.dirname(os.path.abspath(__file__)), 'hwnd.txt'), 'r') as f:
		hwnd = f.read()
		# print(hwnd)

	win32api.SendMessage(hwnd, win32con.WM_APP)
