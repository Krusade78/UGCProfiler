#include "StdAfx.h"
#include "Traduce.h"
#include <ole2.h> 
#include <xmllite.h> 
#include <Shlwapi.h>

#pragma comment(lib, "xmllite.lib")
#pragma comment(lib, "Shlwapi.lib")

CTraduce::TLISTA* CTraduce::xml = NULL;

void CTraduce::Cerrar()
{
	while(xml != NULL)
	{
		TLISTA* siguiente = xml->next;
		delete xml;
		xml = siguiente;
	}
}

int CTraduce::Msg(wchar_t* id1, UINT tipo)
{
	wchar_t* texto1 = NULL;
	Traducir(id1, &texto1);
	return MessageBox(NULL, texto1, NULL, tipo);
}
wchar_t* CTraduce::Txt(wchar_t* id)
{
	wchar_t* texto = NULL;
	Traducir(id, &texto);
	return texto;
}

void CTraduce::Traducir(wchar_t* id, wchar_t** texto)
{
	*texto = (wchar_t*)&cadenaNula;

	if(xml == NULL)
	{
		wchar_t nombre[16] = L"\0";
		wchar_t ruta[MAX_PATH];
		CogerNombreIdioma(nombre);
		if(!BuscarArchivo(nombre, ruta)) return;

		IStream *pFileStream = NULL;
		IXmlReader *pReader = NULL; 
		HRESULT hr;
		if(FAILED(SHCreateStreamOnFile(ruta, STGM_READ, &pFileStream))) return;

		hr = CreateXmlReader(__uuidof(IXmlReader), (void**) &pReader, NULL);
		if (FAILED(hr)) {pFileStream->Release(); pFileStream = NULL;}

		hr = pReader->SetProperty(XmlReaderProperty_DtdProcessing, DtdProcessing_Prohibit);

		hr = pReader->SetInput(pFileStream);
		if (FAILED(hr)) goto mal;

		XmlNodeType nodeType;
		char seccionLauncher = 0;
		TLISTA* finLista = NULL;
		TLISTA* nuevo = NULL;
		hr = pReader->Read(&nodeType);
		while((hr == S_OK) && (seccionLauncher != 2))
		{
			switch(nodeType)
			{
				case XmlNodeType_Element:
				{
					wchar_t* pwszLocalName; 
					hr = pReader->GetLocalName((LPCWSTR*)&pwszLocalName, NULL);
					if(SUCCEEDED(hr))
					{
						if(!seccionLauncher)
						{
							if(_wcsicmp(pwszLocalName, L"Launcher") == 0) seccionLauncher = 1;
						}
						else
						{
							nuevo = new TLISTA;
							wcscpy_s(nuevo->id, 32, pwszLocalName);
							ZeroMemory(nuevo->valor, 512 * sizeof(wchar_t));
							nuevo->next = NULL;
							if(xml == NULL)	xml = nuevo; else finLista->next = nuevo;
							finLista = nuevo;
						}
					}
					break;
				}
				case XmlNodeType_EndElement:
				{
					nuevo = NULL;

					wchar_t* pwszLocalName; 
					hr = pReader->GetLocalName((LPCWSTR*)&pwszLocalName, NULL);
					if(SUCCEEDED(hr))
					{
						if(seccionLauncher)
						{
							if(_wcsicmp(pwszLocalName, L"Launcher") == 0) seccionLauncher = 2;
						}
					}
					break;
				}
				case XmlNodeType_Text: 
				case XmlNodeType_Whitespace:
				{
					if(nuevo != NULL)
					{
						wchar_t* pwszValue; 
						if(SUCCEEDED(pReader->GetValue((LPCWSTR*)&pwszValue, NULL)))
						{
							wcscat_s(nuevo->valor, 511, pwszValue);
						}
					}
				}

			}

			hr = pReader->Read(&nodeType);
		}

mal:
		pReader->Release(); pReader = NULL;
		pFileStream->Release(); pFileStream = NULL;
	}

	ExtraerCadena(id, texto);
}

void CTraduce::CogerNombreIdioma(wchar_t* nombre)
{
	DWORD lcid = 0;
	GetLocaleInfo(LOCALE_USER_DEFAULT, LOCALE_RETURN_NUMBER | LOCALE_IDEFAULTLANGUAGE, (LPTSTR)&lcid, 2);
	switch(lcid & 0xFF)
	{
		case 0xC:
            wcscpy_s(nombre, 16, L"french.xml");
			break;
		case 0x10:
            wcscpy_s(nombre, 16, L"italian.xml");
			break;
		case 0xA:
            wcscpy_s(nombre, 16, L"spanish.xml");
			break;
		case 0x7:
            wcscpy_s(nombre, 16, L"german.xml");
			break;
		default:
            wcscpy_s(nombre, 16, L"english.xml");
	}
}

bool CTraduce::BuscarArchivo(wchar_t* idioma, wchar_t* ruta)
{
	WIN32_FIND_DATA ffi;
	HANDLE hf = FindFirstFile(idioma, &ffi);
	if(hf != INVALID_HANDLE_VALUE)
	{
		wcscpy_s(ruta, MAX_PATH, idioma);
		FindClose(hf);
		return true;
	}
	else
	{
		wcscpy_s(idioma, 16, L"english.xml");
		hf = FindFirstFile(idioma, &ffi);
		if(hf != INVALID_HANDLE_VALUE)
		{
			wcscpy_s(ruta, MAX_PATH, idioma);
			FindClose(hf);
			return true;
		}
		else
		{
			return false;
		}

	}
}

bool CTraduce::ExtraerCadena(wchar_t* id, wchar_t** texto)
{
	TLISTA* pt = xml;
	while(pt != NULL)
	{
		if(_wcsicmp(id, pt->id) == 0)
		{
			*texto = pt->valor;
			break;
		}
		pt = pt->next;
	}
	return true;
}