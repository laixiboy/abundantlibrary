#@brief auto Release Output Files To Other Solution
#@author wolan(khyusj@163.com)

import os

dirs=[os.getcwd(),'\\WLLibrary\\Document\\compiler.bat']

if os.path.exists(''.join(dirs)):
	#execution compiler.bat
	dirs=[os.getcwd(),'\\WLLibrary\\Document\\compiler.bat ',os.getcwd(),'\\WLLibrary\\bin\\Release\\']
	os.system(''.join(dirs))
else:
	#do nothing
	print("do nothing")

print('complete')