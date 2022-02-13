#pragma once
/* Memory leaks detection - define _CRTDBG_MAP_ALLOC as preprocessor macro */
#ifdef _CRTDBG_MAP_ALLOC
#include <stdlib.h>
#include <crtdbg.h>
#endif

//#include <windows.h>
#include <setupapi.h>
#include <wincrypt.h>
#include <stdio.h>
#include <stdint.h>
#include <string.h>
#include "mssign32.h"

//#include <config.h>
#include "msapi_utf8.h"
#include "installer.h"
#include "libwdi.h"
#include "logging.h"
#include "stdfn.h"

class pki
{
public:
	char* winpki_error_str(uint32_t retval);

	/*
 * Convert an UTF8 string to UTF-16 (allocate returned string)
 * Return NULL on error
 */
	static __inline LPWSTR UTF8toWCHAR(LPCSTR szStr)
	{
		int size = 0;
		LPWSTR wszStr = NULL;

		// Find out the size we need to allocate for our converted string
		size = MultiByteToWideChar(CP_UTF8, 0, szStr, -1, NULL, 0);
		if (size <= 1)	// An empty string would be size 1
			return NULL;

		if ((wszStr = (wchar_t*)calloc(size, sizeof(wchar_t))) == NULL)
			return NULL;
		if (MultiByteToWideChar(CP_UTF8, 0, szStr, -1, wszStr, size) != size) {
			free(wszStr);
			return NULL;
		}
		return wszStr;
	}

	BOOL AddCertToStore(PCCERT_CONTEXT pCertContext, LPCSTR szStoreName);
	BOOL RemoveCertFromStore(LPCSTR szCertSubject, LPCSTR szStoreName);
	BOOL AddCertToTrustedPublisher(BYTE* pbCertData, DWORD dwCertSize, BOOL bDisableWarning, HWND hWnd);
	PCCERT_CONTEXT CreateSelfSignedCert(LPCSTR szCertSubject);
	BOOL DeletePrivateKey(PCCERT_CONTEXT pCertContext);
	BOOL SelfSignFile();
};


