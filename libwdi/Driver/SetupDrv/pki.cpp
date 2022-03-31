#include "stdafx.h"
#include "pki.h"

#define KEY_CONTAINER               L"libwdi key container"
#define PF_ERR                      wdi_err
#ifndef CERT_STORE_PROV_SYSTEM_A
#define CERT_STORE_PROV_SYSTEM_A    ((LPCSTR) 9)
#endif
#ifndef szOID_RSA_SHA1RSA
#define szOID_RSA_SHA1RSA           "1.2.840.113549.1.1.5"
#endif
#ifndef szOID_RSA_SHA256RSA
#define szOID_RSA_SHA256RSA         "1.2.840.113549.1.1.11"
#endif

/*
 * Crypt32.dll
 */
typedef HCERTSTORE(WINAPI* CertOpenStore_t)(
	LPCSTR lpszStoreProvider,
	DWORD dwMsgAndCertEncodingType,
	ULONG_PTR hCryptProv,
	DWORD dwFlags,
	const void* pvPara
	);

typedef PCCERT_CONTEXT(WINAPI* CertCreateCertificateContext_t)(
	DWORD dwCertEncodingType,
	const BYTE* pbCertEncoded,
	DWORD cbCertEncoded
	);

typedef PCCERT_CONTEXT(WINAPI* CertFindCertificateInStore_t)(
	HCERTSTORE hCertStore,
	DWORD dwCertEncodingType,
	DWORD dwFindFlags,
	DWORD dwFindType,
	const void* pvFindPara,
	PCCERT_CONTEXT pfPrevCertContext
	);

typedef BOOL(WINAPI* CertAddCertificateContextToStore_t)(
	HCERTSTORE hCertStore,
	PCCERT_CONTEXT pCertContext,
	DWORD dwAddDisposition,
	PCCERT_CONTEXT* pStoreContext
	);

typedef BOOL(WINAPI* CertSetCertificateContextProperty_t)(
	PCCERT_CONTEXT pCertContext,
	DWORD dwPropId,
	DWORD dwFlags,
	const void* pvData
	);

typedef BOOL(WINAPI* CertDeleteCertificateFromStore_t)(
	PCCERT_CONTEXT pCertContext
	);

typedef BOOL(WINAPI* CertFreeCertificateContext_t)(
	PCCERT_CONTEXT pCertContext
	);

typedef BOOL(WINAPI* CertCloseStore_t)(
	HCERTSTORE hCertStore,
	DWORD dwFlags
	);

typedef DWORD(WINAPI* CertGetNameStringA_t)(
	PCCERT_CONTEXT pCertContext,
	DWORD dwType,
	DWORD dwFlags,
	void* pvTypePara,
	LPCSTR pszNameString,
	DWORD cchNameString
	);

typedef BOOL(WINAPI* CryptEncodeObject_t)(
	DWORD dwCertEncodingType,
	LPCSTR lpszStructType,
	const void* pvStructInfo,
	BYTE* pbEncoded,
	DWORD* pcbEncoded
	);

typedef BOOL(WINAPI* CryptDecodeObject_t)(
	DWORD dwCertEncodingType,
	LPCSTR lpszStructType,
	const BYTE* pbEncoded,
	DWORD cbEncoded,
	DWORD dwFlags,
	void* pvStructInfo,
	DWORD* pcbStructInfo
	);

typedef BOOL(WINAPI* CertStrToNameA_t)(
	DWORD dwCertEncodingType,
	LPCSTR pszX500,
	DWORD dwStrType,
	void* pvReserved,
	BYTE* pbEncoded,
	DWORD* pcbEncoded,
	LPCTSTR* ppszError
	);

typedef BOOL(WINAPI* CryptAcquireCertificatePrivateKey_t)(
	PCCERT_CONTEXT pCert,
	DWORD dwFlags,
	void* pvReserved,
	ULONG_PTR* phCryptProvOrNCryptKey,
	DWORD* pdwKeySpec,
	BOOL* pfCallerFreeProvOrNCryptKey
	);

typedef BOOL(WINAPI* CertAddEncodedCertificateToStore_t)(
	HCERTSTORE hCertStore,
	DWORD dwCertEncodingType,
	const BYTE* pbCertEncoded,
	DWORD cbCertEncoded,
	DWORD dwAddDisposition,
	PCCERT_CONTEXT* ppCertContext
	);

// MiNGW32 doesn't know CERT_EXTENSIONS => redef
typedef struct _CERT_EXTENSIONS_ARRAY {
	DWORD cExtension;
	PCERT_EXTENSION rgExtension;
} CERT_EXTENSIONS_ARRAY, * PCERT_EXTENSIONS_ARRAY;

typedef PCCERT_CONTEXT(WINAPI* CertCreateSelfSignCertificate_t)(
	ULONG_PTR hCryptProvOrNCryptKey,
	PCERT_NAME_BLOB pSubjectIssuerBlob,
	DWORD dwFlags,
	PCRYPT_KEY_PROV_INFO pKeyProvInfo,
	PCRYPT_ALGORITHM_IDENTIFIER pSignatureAlgorithm,
	LPSYSTEMTIME pStartTime,
	LPSYSTEMTIME pEndTime,
	PCERT_EXTENSIONS_ARRAY pExtensions
	);

// MinGW32 doesn't have these ones either
#ifndef CERT_ALT_NAME_URL
#define CERT_ALT_NAME_URL 7
#endif
#ifndef CERT_RDN_IA5_STRING
#define CERT_RDN_IA5_STRING 7
#endif
#ifndef szOID_PKIX_POLICY_QUALIFIER_CPS
#define szOID_PKIX_POLICY_QUALIFIER_CPS "1.3.6.1.5.5.7.2.1"
#endif

typedef struct _CERT_POLICY_QUALIFIER_INFO_REDEF {
	LPSTR            pszPolicyQualifierId;
	CRYPT_OBJID_BLOB Qualifier;
} CERT_POLICY_QUALIFIER_INFO_REDEF, * PCERT_POLICY_QUALIFIER_INFO_REDEF;

typedef struct _CERT_POLICY_INFO_ALT {
	LPSTR                             pszPolicyIdentifier;
	DWORD                             cPolicyQualifier;
	PCERT_POLICY_QUALIFIER_INFO_REDEF rgPolicyQualifier;
} CERT_POLICY_INFO_REDEF, * PCERT_POLICY_INFO_REDEF;

typedef struct _CERT_POLICIES_INFO_ARRAY {
	DWORD                   cPolicyInfo;
	PCERT_POLICY_INFO_REDEF rgPolicyInfo;
} CERT_POLICIES_INFO_ARRAY, * PCERT_POLICIES_INFO_ARRAY;

/*
 * WinTrust.dll
 */
#define CRYPTCAT_OPEN_CREATENEW			0x00000001
#define CRYPTCAT_OPEN_ALWAYS			0x00000002

#define CRYPTCAT_ATTR_AUTHENTICATED		0x10000000
#define CRYPTCAT_ATTR_UNAUTHENTICATED	0x20000000
#define CRYPTCAT_ATTR_NAMEASCII			0x00000001
#define CRYPTCAT_ATTR_NAMEOBJID			0x00000002
#define CRYPTCAT_ATTR_DATAASCII			0x00010000
#define CRYPTCAT_ATTR_DATABASE64		0x00020000
#define CRYPTCAT_ATTR_DATAREPLACE		0x00040000

#define SPC_UUID_LENGTH					16
#define SPC_URL_LINK_CHOICE				1
#define SPC_MONIKER_LINK_CHOICE			2
#define SPC_FILE_LINK_CHOICE			3
#define SHA1_HASH_LENGTH				20
#define SPC_PE_IMAGE_DATA_OBJID			"1.3.6.1.4.1.311.2.1.15"
#define SPC_CAB_DATA_OBJID				"1.3.6.1.4.1.311.2.1.25"

typedef BYTE SPC_UUID[SPC_UUID_LENGTH];
typedef struct _SPC_SERIALIZED_OBJECT {
	SPC_UUID ClassId;
	CRYPT_DATA_BLOB SerializedData;
} SPC_SERIALIZED_OBJECT, * PSPC_SERIALIZED_OBJECT;

typedef struct SPC_LINK_ {
	DWORD dwLinkChoice;
	union {
		LPWSTR pwszUrl;
		SPC_SERIALIZED_OBJECT Moniker;
		LPWSTR pwszFile;
	};
} SPC_LINK, * PSPC_LINK;

typedef struct _SPC_PE_IMAGE_DATA {
	CRYPT_BIT_BLOB Flags;
	PSPC_LINK pFile;
} SPC_PE_IMAGE_DATA, * PSPC_PE_IMAGE_DATA;

// MinGW32 doesn't know this one either
typedef struct _CRYPT_ATTRIBUTE_TYPE_VALUE_REDEF {
	LPSTR            pszObjId;
	CRYPT_OBJID_BLOB Value;
} CRYPT_ATTRIBUTE_TYPE_VALUE_REDEF;

typedef struct SIP_INDIRECT_DATA_ {
	CRYPT_ATTRIBUTE_TYPE_VALUE_REDEF Data;
	CRYPT_ALGORITHM_IDENTIFIER       DigestAlgorithm;
	CRYPT_HASH_BLOB                  Digest;
} SIP_INDIRECT_DATA, * PSIP_INDIRECT_DATA;

typedef struct CRYPTCATSTORE_ {
	DWORD      cbStruct;
	DWORD      dwPublicVersion;
	LPWSTR     pwszP7File;
	HCRYPTPROV hProv;
	DWORD      dwEncodingType;
	DWORD      fdwStoreFlags;
	HANDLE     hReserved;
	HANDLE     hAttrs;
	HCRYPTMSG  hCryptMsg;
	HANDLE     hSorted;
} CRYPTCATSTORE;

typedef struct CRYPTCATMEMBER_ {
	DWORD              cbStruct;
	LPWSTR             pwszReferenceTag;
	LPWSTR             pwszFileName;
	GUID               gSubjectType;
	DWORD              fdwMemberFlags;
	PSIP_INDIRECT_DATA pIndirectData;
	DWORD              dwCertVersion;
	DWORD              dwReserved;
	HANDLE             hReserved;
	CRYPT_ATTR_BLOB    sEncodedIndirectData;
	CRYPT_ATTR_BLOB    sEncodedMemberInfo;
} CRYPTCATMEMBER;

typedef struct CRYPTCATATTRIBUTE_ {
	DWORD  cbStruct;
	LPWSTR pwszReferenceTag;
	DWORD  dwAttrTypeAndAction;
	DWORD  cbValue;
	BYTE* pbValue;
	DWORD  dwReserved;
} CRYPTCATATTRIBUTE;

typedef HANDLE(WINAPI* CryptCATOpen_t)(
	LPWSTR pwszFileName,
	DWORD fdwOpenFlags,
	ULONG_PTR hProv,
	DWORD dwPublicVersion,
	DWORD dwEncodingType
	);

typedef BOOL(WINAPI* CryptCATClose_t)(
	HANDLE hCatalog
	);

typedef CRYPTCATSTORE* (WINAPI* CryptCATStoreFromHandle_t)(
	HANDLE hCatalog
	);

typedef CRYPTCATATTRIBUTE* (WINAPI* CryptCATEnumerateCatAttr_t)(
	HANDLE hCatalog,
	CRYPTCATATTRIBUTE* pPrevAttr
	);

typedef CRYPTCATATTRIBUTE* (WINAPI* CryptCATPutCatAttrInfo_t)(
	HANDLE hCatalog,
	LPWSTR pwszReferenceTag,
	DWORD dwAttrTypeAndAction,
	DWORD cbData,
	BYTE* pbData
	);

typedef CRYPTCATMEMBER* (WINAPI* CryptCATEnumerateMember_t)(
	HANDLE hCatalog,
	CRYPTCATMEMBER* pPrevMember
	);

typedef CRYPTCATMEMBER* (WINAPI* CryptCATPutMemberInfo_t)(
	HANDLE hCatalog,
	LPWSTR pwszFileName,
	LPWSTR pwszReferenceTag,
	GUID* pgSubjectType,
	DWORD dwCertVersion,
	DWORD cbSIPIndirectData,
	BYTE* pbSIPIndirectData
	);

typedef CRYPTCATATTRIBUTE* (WINAPI* CryptCATEnumerateAttr_t)(
	HANDLE hCatalog,
	CRYPTCATMEMBER* pCatMember,
	CRYPTCATATTRIBUTE* pPrevAttr
	);

typedef CRYPTCATATTRIBUTE* (WINAPI* CryptCATPutAttrInfo_t)(
	HANDLE hCatalog,
	CRYPTCATMEMBER* pCatMember,
	LPWSTR pwszReferenceTag,
	DWORD dwAttrTypeAndAction,
	DWORD cbData,
	BYTE* pbData
	);

typedef BOOL(WINAPI* CryptCATPersistStore_t)(
	HANDLE hCatalog
	);

typedef BOOL(WINAPI* CryptCATAdminCalcHashFromFileHandle_t)(
	HANDLE hFile,
	DWORD* pcbHash,
	BYTE* pbHash,
	DWORD dwFlags
	);

extern char* windows_error_str(uint32_t retval);

/*
 * FormatMessage does not handle PKI errors
 */
char* pki::winpki_error_str(uint32_t retval)
{
	static char error_string[64];
	uint32_t error_code = retval ? retval : GetLastError();

	if (error_code == 0x800706D9)
		return "This system is missing required cryptographic services.";
	if (error_code == 0x80070020)
		return "Sharing violation - Some data handles to this file are still open.";

	if ((error_code >> 16) != 0x8009)
	{
		static_sprintf(error_string, "Windows error 0x%X", error_code);
		return error_string;
		//return windows_error_str(error_code);
	}

	switch (error_code) {
	case NTE_BAD_UID:
		return "Bad UID.";
	case NTE_BAD_KEYSET:
		return "Keyset does not exist.";
	case NTE_KEYSET_ENTRY_BAD:
		return "Keyset as registered is invalid.";
	case NTE_BAD_FLAGS:
		return "Invalid flags specified.";
	case NTE_BAD_KEYSET_PARAM:
		return "The Keyset parameter is invalid.";
	case NTE_BAD_PROV_TYPE:
		return "Invalid provider type specified.";
	case NTE_EXISTS:
		return "Object already exists.";
	case NTE_BAD_SIGNATURE:
		return "Invalid Signature.";
	case NTE_PROVIDER_DLL_FAIL:
		return "Provider DLL failed to initialize correctly.";
	case NTE_SIGNATURE_FILE_BAD:
		return "The digital signature file is corrupt.";
	case NTE_PROV_DLL_NOT_FOUND:
		return "Provider DLL could not be found.";
	case NTE_KEYSET_NOT_DEF:
		return "The keyset is not defined.";
	case NTE_NO_MEMORY:
		return "Insufficient memory available for the operation.";
	case CRYPT_E_MSG_ERROR:
		return "An error occurred while performing an operation on a cryptographic message.";
	case CRYPT_E_UNKNOWN_ALGO:
		return "Unknown cryptographic algorithm.";
	case CRYPT_E_INVALID_MSG_TYPE:
		return "Invalid cryptographic message type.";
	case CRYPT_E_HASH_VALUE:
		return "The hash value is not correct";
	case CRYPT_E_ISSUER_SERIALNUMBER:
		return "Invalid issuer and/or serial number.";
	case CRYPT_E_BAD_LEN:
		return "The length specified for the output data was insufficient.";
	case CRYPT_E_BAD_ENCODE:
		return "An error occurred during encode or decode operation.";
	case CRYPT_E_FILE_ERROR:
		return "An error occurred while reading or writing to a file.";
	case CRYPT_E_NOT_FOUND:
		return "Cannot find object or property.";
	case CRYPT_E_EXISTS:
		return "The object or property already exists.";
	case CRYPT_E_NO_PROVIDER:
		return "No provider was specified for the store or object.";
	case CRYPT_E_DELETED_PREV:
		return "The previous certificate or CRL context was deleted.";
	case CRYPT_E_NO_MATCH:
		return "Cannot find the requested object.";
	case CRYPT_E_UNEXPECTED_MSG_TYPE:
		return "The certificate does not have a property that references a private key.";
	case CRYPT_E_NO_KEY_PROPERTY:
		return "Cannot find the private key to use for decryption.";
	case CRYPT_E_NO_DECRYPT_CERT:
		return "Cannot find the certificate to use for decryption.";
	case CRYPT_E_BAD_MSG:
		return "Not a cryptographic message.";
	case CRYPT_E_NO_SIGNER:
		return "The signed cryptographic message does not have a signer for the specified signer index.";
	case CRYPT_E_REVOKED:
		return "The certificate is revoked.";
	case CRYPT_E_NO_REVOCATION_DLL:
		return "No Dll or exported function was found to verify revocation.";
	case CRYPT_E_NO_REVOCATION_CHECK:
		return "The revocation function was unable to check revocation for the certificate.";
	case CRYPT_E_REVOCATION_OFFLINE:
		return "The revocation function was unable to check revocation because the revocation server was offline.";
	case CRYPT_E_NOT_IN_REVOCATION_DATABASE:
		return "The certificate is not in the revocation server's database.";
	case CRYPT_E_INVALID_NUMERIC_STRING:
	case CRYPT_E_INVALID_PRINTABLE_STRING:
	case CRYPT_E_INVALID_IA5_STRING:
	case CRYPT_E_INVALID_X500_STRING:
	case CRYPT_E_NOT_CHAR_STRING:
		return "Invalid string.";
	case CRYPT_E_SECURITY_SETTINGS:
		return "The cryptographic operation failed due to a local security option setting.";
	case CRYPT_E_NO_VERIFY_USAGE_CHECK:
		return "The called function was unable to do a usage check on the subject.";
	case CRYPT_E_VERIFY_USAGE_OFFLINE:
		return "Since the server was offline, the called function was unable to complete the usage check.";
	case CRYPT_E_NO_TRUSTED_SIGNER:
		return "None of the signers of the cryptographic message or certificate trust list is trusted.";
	default:
		static_sprintf(error_string, "Unknown PKI error 0x%08X", error_code);
		return error_string;
	}
}



/*
 * Parts of the following functions are based on:
 * http://blogs.msdn.com/b/alejacma/archive/2009/03/16/how-to-create-a-self-signed-certificate-with-cryptoapi-c.aspx
 * http://blogs.msdn.com/b/alejacma/archive/2008/12/11/how-to-sign-exe-files-with-an-authenticode-certificate-part-2.aspx
 * http://www.jensign.com/hash/index.html
 */

 /*
  * Add a certificate, identified by its pCertContext, to the system store 'szStoreName'
  */
BOOL pki::AddCertToStore(PCCERT_CONTEXT pCertContext, LPCSTR szStoreName)
{
	PF_DECL_LOAD_LIBRARY(Crypt32);
	PF_DECL(CertOpenStore);
	PF_DECL(CertSetCertificateContextProperty);
	PF_DECL(CertAddCertificateContextToStore);
	PF_DECL(CertCloseStore);
	HCERTSTORE hSystemStore = NULL;
	CRYPT_DATA_BLOB libwdiNameBlob = { 14, (BYTE*)L"libwdi" };
	BOOL r = FALSE;

	PF_INIT_OR_OUT(CertOpenStore, Crypt32);
	PF_INIT_OR_OUT(CertSetCertificateContextProperty, Crypt32);
	PF_INIT_OR_OUT(CertAddCertificateContextToStore, Crypt32);
	PF_INIT_OR_OUT(CertCloseStore, Crypt32);

	hSystemStore = pfCertOpenStore(CERT_STORE_PROV_SYSTEM_A, X509_ASN_ENCODING,
		0, CERT_SYSTEM_STORE_LOCAL_MACHINE, szStoreName);
	if (hSystemStore == NULL) {
		wdi_warn("Failed to open system store '%s': %s", szStoreName, winpki_error_str(0));
		goto out;
	}

	if (!pfCertSetCertificateContextProperty(pCertContext, CERT_FRIENDLY_NAME_PROP_ID, 0, &libwdiNameBlob)) {
		wdi_warn("Could not set friendly name: %s", winpki_error_str(0));
		goto out;
	}

	if (!pfCertAddCertificateContextToStore(hSystemStore, pCertContext, CERT_STORE_ADD_REPLACE_EXISTING, NULL)) {
		wdi_warn("Failed to add certificate to system store '%s': %s", szStoreName, winpki_error_str(0));
		goto out;
	}
	r = TRUE;

out:
	if (hSystemStore != NULL)
		pfCertCloseStore(hSystemStore, 0);
	PF_FREE_LIBRARY(Crypt32);
	return r;
}

/*
 * Remove a certificate, identified by its subject, to the system store 'szStoreName'
 */
BOOL pki::RemoveCertFromStore(LPCSTR szCertSubject, LPCSTR szStoreName)
{
	PF_DECL_LOAD_LIBRARY(Crypt32);
	PF_DECL(CertOpenStore);
	PF_DECL(CertFindCertificateInStore);
	PF_DECL(CertDeleteCertificateFromStore);
	PF_DECL(CertCloseStore);
	PF_DECL(CertStrToNameA);
	HCERTSTORE hSystemStore = NULL;
	PCCERT_CONTEXT pCertContext;
	CERT_NAME_BLOB certNameBlob = { 0, NULL };
	BOOL r = FALSE;

	PF_INIT_OR_OUT(CertOpenStore, Crypt32);
	PF_INIT_OR_OUT(CertFindCertificateInStore, Crypt32);
	PF_INIT_OR_OUT(CertDeleteCertificateFromStore, Crypt32);
	PF_INIT_OR_OUT(CertCloseStore, Crypt32);
	PF_INIT_OR_OUT(CertStrToNameA, Crypt32);

	hSystemStore = pfCertOpenStore(CERT_STORE_PROV_SYSTEM_A, X509_ASN_ENCODING,
		0, CERT_SYSTEM_STORE_LOCAL_MACHINE, szStoreName);
	if (hSystemStore == NULL) {
		wdi_warn("failed to open system store '%s': %s", szStoreName, winpki_error_str(0));
		goto out;
	}

	// Encode Cert Name
	if ((!pfCertStrToNameA(X509_ASN_ENCODING, szCertSubject, CERT_X500_NAME_STR, NULL, NULL, &certNameBlob.cbData, NULL))
		|| ((certNameBlob.pbData = (BYTE*)malloc(certNameBlob.cbData)) == NULL)
		|| (!pfCertStrToNameA(X509_ASN_ENCODING, szCertSubject, CERT_X500_NAME_STR, NULL, certNameBlob.pbData, &certNameBlob.cbData, NULL))) {
		wdi_warn("Failed to encode '%s': %s", szCertSubject, winpki_error_str(0));
		goto out;
	}

	pCertContext = NULL;
	while ((pCertContext = pfCertFindCertificateInStore(hSystemStore, X509_ASN_ENCODING, 0,
		CERT_FIND_SUBJECT_NAME, (const void*)&certNameBlob, NULL)) != NULL) {
		if (!pfCertDeleteCertificateFromStore(pCertContext)) {
			wdi_warn("Failed to delete certificate '%s' from '%s' store: %s",
				szCertSubject, szStoreName, winpki_error_str(0));
			goto out;
		}
		wdi_info("Deleted existing certificate '%s' from '%s' store", szCertSubject, szStoreName);
	}
	r = TRUE;

out:
	free(certNameBlob.pbData);
	if (hSystemStore != NULL)
		pfCertCloseStore(hSystemStore, 0);
	PF_FREE_LIBRARY(Crypt32);
	return r;
}

/*
 * Add certificate data to the TrustedPublisher system store
 * Unless bDisableWarning is set, warn the user before install
 */
BOOL pki::AddCertToTrustedPublisher(BYTE* pbCertData, DWORD dwCertSize, BOOL bDisableWarning, HWND hWnd)
{
	PF_DECL_LOAD_LIBRARY(Crypt32);
	PF_DECL(CertOpenStore);
	PF_DECL(CertCreateCertificateContext);
	PF_DECL(CertFindCertificateInStore);
	PF_DECL(CertAddCertificateContextToStore);
	PF_DECL(CertFreeCertificateContext);
	PF_DECL(CertGetNameStringA);
	PF_DECL(CertCloseStore);
	BOOL r = FALSE;
	int user_input;
	HCERTSTORE hSystemStore = NULL;
	PCCERT_CONTEXT pCertContext = NULL, pStoreCertContext = NULL;
	char org[MAX_PATH], org_unit[MAX_PATH];
	char msg_string[1024];

	PF_INIT_OR_OUT(CertOpenStore, Crypt32);
	PF_INIT_OR_OUT(CertCreateCertificateContext, Crypt32);
	PF_INIT_OR_OUT(CertFindCertificateInStore, Crypt32);
	PF_INIT_OR_OUT(CertAddCertificateContextToStore, Crypt32);
	PF_INIT_OR_OUT(CertFreeCertificateContext, Crypt32);
	PF_INIT_OR_OUT(CertGetNameStringA, Crypt32);
	PF_INIT_OR_OUT(CertCloseStore, Crypt32);

	hSystemStore = pfCertOpenStore(CERT_STORE_PROV_SYSTEM_A, X509_ASN_ENCODING,
		0, CERT_SYSTEM_STORE_LOCAL_MACHINE, "TrustedPublisher");

	if (hSystemStore == NULL) {
		wdi_warn("Unable to open system store: %s", winpki_error_str(0));
		goto out;
	}

	/* Check whether certificate already exists
	 * We have to do this manually, so that we can produce a warning to the user
	 * before any certificate is added to the store (first time or update)
	 */
	pCertContext = pfCertCreateCertificateContext(X509_ASN_ENCODING, pbCertData, dwCertSize);

	if (pCertContext == NULL) {
		wdi_warn("Could not create context for certificate: %s", winpki_error_str(0));
		pfCertCloseStore(hSystemStore, 0);
		goto out;
	}

	pStoreCertContext = pfCertFindCertificateInStore(hSystemStore, X509_ASN_ENCODING, 0,
		CERT_FIND_EXISTING, (const void*)pCertContext, NULL);
	if (pStoreCertContext == NULL) {
		user_input = IDOK;
		if (!bDisableWarning) {
			org[0] = 0; org_unit[0] = 0;
			pfCertGetNameStringA(pCertContext, CERT_NAME_ATTR_TYPE, 0, szOID_ORGANIZATION_NAME, org, sizeof(org));
			pfCertGetNameStringA(pCertContext, CERT_NAME_ATTR_TYPE, 0, szOID_ORGANIZATIONAL_UNIT_NAME, org_unit, sizeof(org_unit));
			static_sprintf(msg_string, "Warning: this software is about to install the following organization\n"
				"as a Trusted Publisher on your system:\n\n '%s%s%s%s'\n\n"
				"This will allow this Publisher to run software with elevated privileges,\n"
				"as well as install driver packages, without further security notices.\n\n"
				"If this is not what you want, you can cancel this operation now.", org,
				(org_unit[0] != 0) ? " (" : "", org_unit, (org_unit[0] != 0) ? ")" : "");
			user_input = MessageBoxA(hWnd, msg_string,
				"Warning: Trusted Certificate installation", MB_OKCANCEL | MB_ICONWARNING);
		}
		if (user_input != IDOK) {
			wdi_info("Operation cancelled by the user");
		}
		else {
			if (!pfCertAddCertificateContextToStore(hSystemStore, pCertContext, CERT_STORE_ADD_NEWER, NULL)) {
				wdi_warn("Could not add certificate: %s", winpki_error_str(0));
			}
			else {
				r = TRUE;
			}
		}
	}
	else {
		r = TRUE;	// Cert already exists
	}

out:
	if (pCertContext != NULL)
		pfCertFreeCertificateContext(pCertContext);
	if (pStoreCertContext != NULL)
		pfCertFreeCertificateContext(pStoreCertContext);
	if (hSystemStore)
		pfCertCloseStore(hSystemStore, 0);
	PF_FREE_LIBRARY(Crypt32);
	return r;
}

/*
 * Create a self signed certificate for code signing
 */
PCCERT_CONTEXT pki::CreateSelfSignedCert(LPCSTR szCertSubject)
{
	PF_DECL_LOAD_LIBRARY(Crypt32);
	PF_DECL(CryptEncodeObject);
	PF_DECL(CertStrToNameA);
	PF_DECL(CertCreateSelfSignCertificate);
	PF_DECL(CertFreeCertificateContext);

	DWORD dwSize = 0;
	HCRYPTPROV hCSP = 0;
	HCRYPTKEY hKey = 0;
	PCCERT_CONTEXT pCertContext = NULL;
	CERT_NAME_BLOB SubjectIssuerBlob = { 0, NULL };
	CRYPT_KEY_PROV_INFO KeyProvInfo;
	CRYPT_ALGORITHM_IDENTIFIER SignatureAlgorithm;
	LPWSTR wszKeyContainer = KEY_CONTAINER;
	LPBYTE pbEnhKeyUsage = NULL, pbAltNameInfo = NULL, pbCPSNotice = NULL, pbPolicyInfo = NULL;
	SYSTEMTIME sExpirationDate = { 2029, 01, 01, 01, 00, 00, 00, 000 };
	CERT_EXTENSION certExtension[3];
	CERT_EXTENSIONS_ARRAY certExtensionsArray;
	// Code Signing Enhanced Key Usage
	LPSTR szCertPolicyElementId = "1.3.6.1.5.5.7.3.3"; // szOID_PKIX_KP_CODE_SIGNING;
	CERT_ENHKEY_USAGE certEnhKeyUsage = { 1, &szCertPolicyElementId };
	// Abuse Alt Name to insert ourselves in the e-mail field
	CERT_ALT_NAME_ENTRY certAltNameEntry = { CERT_ALT_NAME_RFC822_NAME,
		{ (PCERT_OTHER_NAME)L"Created by libwdi (http://libwdi.akeo.ie)" } };
	CERT_ALT_NAME_INFO certAltNameInfo = { 1, &certAltNameEntry };
	// Certificate Policies
	CERT_POLICY_QUALIFIER_INFO_REDEF certPolicyQualifier;
	CERT_POLICY_INFO_REDEF certPolicyInfo = { "1.3.6.1.5.5.7.2.1", 1, &certPolicyQualifier };
	CERT_POLICIES_INFO_ARRAY certPolicyInfoArray = { 1, &certPolicyInfo };
	CHAR szCPSName[] = "http://libwdi-cps.akeo.ie";
	CERT_NAME_VALUE certCPSValue;

	PF_INIT_OR_OUT(CryptEncodeObject, Crypt32);
	PF_INIT_OR_OUT(CertStrToNameA, Crypt32);
	PF_INIT_OR_OUT(CertCreateSelfSignCertificate, Crypt32);
	PF_INIT_OR_OUT(CertFreeCertificateContext, Crypt32);

	// Set Enhanced Key Usage extension to Code Signing only
	if ((!pfCryptEncodeObject(X509_ASN_ENCODING, X509_ENHANCED_KEY_USAGE, (LPVOID)&certEnhKeyUsage, NULL, &dwSize))
		|| ((pbEnhKeyUsage = (BYTE*)malloc(dwSize)) == NULL)
		|| (!pfCryptEncodeObject(X509_ASN_ENCODING, X509_ENHANCED_KEY_USAGE, (LPVOID)&certEnhKeyUsage, pbEnhKeyUsage, &dwSize))) {
		wdi_warn("Could not setup EKU for code signing: %s", winpki_error_str(0));
		goto out;
	}
	certExtension[0].pszObjId = szOID_ENHANCED_KEY_USAGE;
	certExtension[0].fCritical = TRUE;		// only allow code signing
	certExtension[0].Value.cbData = dwSize;
	certExtension[0].Value.pbData = pbEnhKeyUsage;

	// Set Alt Name parameter
	if ((!pfCryptEncodeObject(X509_ASN_ENCODING, X509_ALTERNATE_NAME, (LPVOID)&certAltNameInfo, NULL, &dwSize))
		|| ((pbAltNameInfo = (BYTE*)malloc(dwSize)) == NULL)
		|| (!pfCryptEncodeObject(X509_ASN_ENCODING, X509_ALTERNATE_NAME, (LPVOID)&certAltNameInfo, pbAltNameInfo, &dwSize))) {
		wdi_warn("Could not set Alt Name: %s", winpki_error_str(0));
		goto out;
	}
	certExtension[1].pszObjId = szOID_SUBJECT_ALT_NAME;
	certExtension[1].fCritical = FALSE;
	certExtension[1].Value.cbData = dwSize;
	certExtension[1].Value.pbData = pbAltNameInfo;

	// Set the CPS Certificate Policies field - this enables the "Issuer Statement" button on the cert
	certCPSValue.dwValueType = CERT_RDN_IA5_STRING;
	certCPSValue.Value.cbData = sizeof(szCPSName);
	certCPSValue.Value.pbData = (BYTE*)szCPSName;
	if ((!pfCryptEncodeObject(X509_ASN_ENCODING, X509_NAME_VALUE, (LPVOID)&certCPSValue, NULL, &dwSize))
		|| ((pbCPSNotice = (BYTE*)malloc(dwSize)) == NULL)
		|| (!pfCryptEncodeObject(X509_ASN_ENCODING, X509_NAME_VALUE, (LPVOID)&certCPSValue, pbCPSNotice, &dwSize))) {
		wdi_warn("Could not setup CPS: %s", winpki_error_str(0));
		goto out;
	}

	certPolicyQualifier.pszPolicyQualifierId = szOID_PKIX_POLICY_QUALIFIER_CPS;
	certPolicyQualifier.Qualifier.cbData = dwSize;
	certPolicyQualifier.Qualifier.pbData = pbCPSNotice;
	if ((!pfCryptEncodeObject(X509_ASN_ENCODING, X509_CERT_POLICIES, (LPVOID)&certPolicyInfoArray, NULL, &dwSize))
		|| ((pbPolicyInfo = (BYTE*)malloc(dwSize)) == NULL)
		|| (!pfCryptEncodeObject(X509_ASN_ENCODING, X509_CERT_POLICIES, (LPVOID)&certPolicyInfoArray, pbPolicyInfo, &dwSize))) {
		wdi_warn("Could not setup Certificate Policies: %s", winpki_error_str(0));
		goto out;
	}
	certExtension[2].pszObjId = szOID_CERT_POLICIES;
	certExtension[2].fCritical = FALSE;
	certExtension[2].Value.cbData = dwSize;
	certExtension[2].Value.pbData = pbPolicyInfo;

	certExtensionsArray.cExtension = ARRAYSIZE(certExtension);
	certExtensionsArray.rgExtension = certExtension;
	wdi_dbg("Set Enhanced Key Usage, URL and CPS");

	if (CryptAcquireContextW(&hCSP, wszKeyContainer, NULL, PROV_RSA_FULL, CRYPT_MACHINE_KEYSET | CRYPT_SILENT)) {
		wdi_dbg("Acquired existing key container");
	}
	else if ((GetLastError() == NTE_BAD_KEYSET || GetLastError() == NTE_KEYSET_ENTRY_BAD)
		&& (CryptAcquireContextW(&hCSP, wszKeyContainer, NULL, PROV_RSA_FULL, CRYPT_NEWKEYSET | CRYPT_MACHINE_KEYSET | CRYPT_SILENT))) {
		wdi_dbg("Created new key container");
	}
	else {
		wdi_warn("Could not obtain a key container: %s (0x%08X)", winpki_error_str(0), GetLastError());
		goto out;
	}

	// Generate key pair using RSA 4096
	// (Key_size <<16) because key size is in upper 16 bits
	if (!CryptGenKey(hCSP, AT_SIGNATURE, (4096U << 16) | CRYPT_EXPORTABLE, &hKey)) {
		wdi_dbg("Could not generate keypair: %s", winpki_error_str(0));
		goto out;
	}
	wdi_dbg("Generated new keypair...");

	// Set the subject
	if ((!pfCertStrToNameA(X509_ASN_ENCODING, szCertSubject, CERT_X500_NAME_STR, NULL, NULL, &SubjectIssuerBlob.cbData, NULL))
		|| ((SubjectIssuerBlob.pbData = (BYTE*)malloc(SubjectIssuerBlob.cbData)) == NULL)
		|| (!pfCertStrToNameA(X509_ASN_ENCODING, szCertSubject, CERT_X500_NAME_STR, NULL, SubjectIssuerBlob.pbData, &SubjectIssuerBlob.cbData, NULL))) {
		wdi_warn("Could not encode subject name for self signed cert: %s", winpki_error_str(0));
		goto out;
	}

	// Prepare key provider structure for self-signed certificate
	memset(&KeyProvInfo, 0, sizeof(KeyProvInfo));
	KeyProvInfo.pwszContainerName = wszKeyContainer;
	KeyProvInfo.pwszProvName = NULL;
	KeyProvInfo.dwProvType = PROV_RSA_FULL;
	KeyProvInfo.dwFlags = CRYPT_MACHINE_KEYSET;
	KeyProvInfo.cProvParam = 0;
	KeyProvInfo.rgProvParam = NULL;
	KeyProvInfo.dwKeySpec = AT_SIGNATURE;

	// Prepare algorithm structure for self-signed certificate
	memset(&SignatureAlgorithm, 0, sizeof(SignatureAlgorithm));

	SignatureAlgorithm.pszObjId = szOID_RSA_SHA256RSA;

	// Create self-signed certificate
	pCertContext = pfCertCreateSelfSignCertificate((ULONG_PTR)NULL,
		&SubjectIssuerBlob, 0, &KeyProvInfo, &SignatureAlgorithm, NULL, &sExpirationDate, &certExtensionsArray);
	if (pCertContext == NULL) {
		wdi_warn("Could not create self signed certificate: %s", winpki_error_str(0));
		goto out;
	}
	wdi_info("Created new self-signed certificate '%s'", szCertSubject);

out:
	free(pbEnhKeyUsage);
	free(pbAltNameInfo);
	free(pbCPSNotice);
	free(pbPolicyInfo);
	free(SubjectIssuerBlob.pbData);
	if (hKey)
		CryptDestroyKey(hKey);
	if (hCSP)
		CryptReleaseContext(hCSP, 0);
	PF_FREE_LIBRARY(Crypt32);
	return pCertContext;
}

/*
 * Delete the private key associated with a specific cert
 */
BOOL pki::DeletePrivateKey(PCCERT_CONTEXT pCertContext)
{
	PF_DECL_LOAD_LIBRARY(Crypt32);
	PF_DECL(CryptAcquireCertificatePrivateKey);
	PF_DECL(CertOpenStore);
	PF_DECL(CertCloseStore);
	PF_DECL(CertAddEncodedCertificateToStore);
	PF_DECL(CertSetCertificateContextProperty);
	PF_DECL(CertFreeCertificateContext);

	LPWSTR wszKeyContainer = KEY_CONTAINER;
	HCRYPTPROV hCSP = 0;
	DWORD dwKeySpec;
	BOOL bFreeCSP = FALSE, r = FALSE;
	HCERTSTORE hSystemStore;
	LPCSTR szStoresToUpdate[2] = { "Root", "TrustedPublisher" };
	CRYPT_DATA_BLOB libwdiNameBlob = { 14, (BYTE*)L"libwdi" };
	PCCERT_CONTEXT pCertContextUpdate = NULL;
	int i;

	PF_INIT_OR_OUT(CryptAcquireCertificatePrivateKey, Crypt32);
	PF_INIT_OR_OUT(CertOpenStore, Crypt32);
	PF_INIT_OR_OUT(CertCloseStore, Crypt32);
	PF_INIT_OR_OUT(CertAddEncodedCertificateToStore, Crypt32);
	PF_INIT_OR_OUT(CertSetCertificateContextProperty, Crypt32);
	PF_INIT_OR_OUT(CertFreeCertificateContext, Crypt32);

	if (!pfCryptAcquireCertificatePrivateKey(pCertContext, CRYPT_ACQUIRE_SILENT_FLAG, NULL, &hCSP, &dwKeySpec, &bFreeCSP)) {
		wdi_warn("Error getting CSP: %s", winpki_error_str(0));
		goto out;
	}

	if (!CryptAcquireContextW(&hCSP, wszKeyContainer, NULL, PROV_RSA_FULL, CRYPT_MACHINE_KEYSET | CRYPT_SILENT | CRYPT_DELETEKEYSET)) {
		wdi_warn("Failed to delete private key: %s", winpki_error_str(0));
	}

	// This is optional, but unless we reimport the cert data after having deleted the key
	// end users will still see a "You have a private key that corresponds to this certificate" message.
	for (i = 0; i < ARRAYSIZE(szStoresToUpdate); i++)
	{
		hSystemStore = pfCertOpenStore(CERT_STORE_PROV_SYSTEM_A, X509_ASN_ENCODING,
			0, CERT_SYSTEM_STORE_LOCAL_MACHINE, szStoresToUpdate[i]);
		if (hSystemStore == NULL) continue;

		if ((pfCertAddEncodedCertificateToStore(hSystemStore, X509_ASN_ENCODING, pCertContext->pbCertEncoded,
			pCertContext->cbCertEncoded, CERT_STORE_ADD_REPLACE_EXISTING, &pCertContextUpdate)) && (pCertContextUpdate != NULL)) {
			// The friendly name is lost in this operation - restore it
			if (!pfCertSetCertificateContextProperty(pCertContextUpdate, CERT_FRIENDLY_NAME_PROP_ID, 0, &libwdiNameBlob)) {
				wdi_warn("Could not set friendly name: %s", winpki_error_str(0));
			}
			pfCertFreeCertificateContext(pCertContextUpdate);
		}
		else {
			wdi_warn("Failed to update '%s': %s", szStoresToUpdate[i], winpki_error_str(0));
		}
		pfCertCloseStore(hSystemStore, 0);
	}

	r = TRUE;

out:
	if ((bFreeCSP) && (hCSP)) {
		CryptReleaseContext(hCSP, 0);
	}
	PF_FREE_LIBRARY(Crypt32);
	return r;
}

/*
 * Digitally sign a file and make it system-trusted by:
 * - creating a self signed certificate for code signing
 * - adding this certificate to both the Root and TrustedPublisher system stores
 * - signing the file provided
 * - deleting the self signed certificate private key so that it cannot be reused
 */
BOOL pki::SelfSignFile()
{
	PF_DECL_LOAD_LIBRARY(MSSign32);
	PF_DECL_LOAD_LIBRARY(Crypt32);
	PF_DECL(SignerSignEx);
	PF_DECL(SignerFreeSignerContext);
	PF_DECL(CertFreeCertificateContext);
	PF_DECL(CertCloseStore);

	BOOL r = FALSE;
	LPWSTR wszFileName = NULL;
	HRESULT hResult = S_OK;
	PCCERT_CONTEXT pCertContext = NULL;
	DWORD dwIndex;
	SIGNER_FILE_INFO signerFileInfo;
	SIGNER_SUBJECT_INFO signerSubjectInfo;
	SIGNER_CERT_STORE_INFO signerCertStoreInfo;
	SIGNER_CERT signerCert;
	SIGNER_SIGNATURE_INFO signerSignatureInfo;
	PSIGNER_CONTEXT pSignerContext = NULL;
	CRYPT_ATTRIBUTES_ARRAY cryptAttributesArray;
	CRYPT_ATTRIBUTE cryptAttribute[2];
	CRYPT_INTEGER_BLOB oidSpOpusInfoBlob, oidStatementTypeBlob;
	BYTE pbOidSpOpusInfo[] = SP_OPUS_INFO_DATA;
	BYTE pbOidStatementType[] = STATEMENT_TYPE_DATA;
	LPCSTR szCertSubject = "CN=Alfredo Costalago";

	PF_INIT_OR_OUT(SignerSignEx, MSSign32);
	PF_INIT_OR_OUT(SignerFreeSignerContext, MSSign32);
	PF_INIT_OR_OUT(CertFreeCertificateContext, Crypt32);
	PF_INIT_OR_OUT(CertCloseStore, Crypt32);

	// Delete any previous certificate with the same subject
	RemoveCertFromStore(szCertSubject, "Root");
	RemoveCertFromStore(szCertSubject, "TrustedPublisher");

	pCertContext = CreateSelfSignedCert(szCertSubject);
	if (pCertContext == NULL) {
		goto out;
	}
	wdi_dbg("Successfully created certificate '%s'", szCertSubject);
	if ((!AddCertToStore(pCertContext, "Root"))
		|| (!AddCertToStore(pCertContext, "TrustedPublisher"))) {
		goto out;
	}
	wdi_info("Added certificate '%s' to 'Root' and 'TrustedPublisher' stores", szCertSubject);

	// Setup SIGNER_FILE_INFO struct
	signerFileInfo.cbSize = sizeof(SIGNER_FILE_INFO);
	signerFileInfo.hFile = NULL;

	// Prepare SIGNER_SUBJECT_INFO struct
	signerSubjectInfo.cbSize = sizeof(SIGNER_SUBJECT_INFO);
	dwIndex = 0;
	signerSubjectInfo.pdwIndex = &dwIndex;
	signerSubjectInfo.dwSubjectChoice = SIGNER_SUBJECT_FILE;
	signerSubjectInfo.pSignerFileInfo = &signerFileInfo;

	// Prepare SIGNER_CERT_STORE_INFO struct
	signerCertStoreInfo.cbSize = sizeof(SIGNER_CERT_STORE_INFO);
	signerCertStoreInfo.pSigningCert = pCertContext;
	signerCertStoreInfo.dwCertPolicy = SIGNER_CERT_POLICY_CHAIN;
	signerCertStoreInfo.hCertStore = NULL;

	// Prepare SIGNER_CERT struct
	signerCert.cbSize = sizeof(SIGNER_CERT);
	signerCert.dwCertChoice = SIGNER_CERT_STORE;
	signerCert.pCertStoreInfo = &signerCertStoreInfo;
	signerCert.hwnd = NULL;

	// Prepare the additional Authenticode OIDs
	oidSpOpusInfoBlob.cbData = sizeof(pbOidSpOpusInfo);
	oidSpOpusInfoBlob.pbData = pbOidSpOpusInfo;
	oidStatementTypeBlob.cbData = sizeof(pbOidStatementType);
	oidStatementTypeBlob.pbData = pbOidStatementType;
	cryptAttribute[0].cValue = 1;
	cryptAttribute[0].rgValue = &oidSpOpusInfoBlob;
	cryptAttribute[0].pszObjId = "1.3.6.1.4.1.311.2.1.12"; // SPC_SP_OPUS_INFO_OBJID in wintrust.h
	cryptAttribute[1].cValue = 1;
	cryptAttribute[1].rgValue = &oidStatementTypeBlob;
	cryptAttribute[1].pszObjId = "1.3.6.1.4.1.311.2.1.11"; // SPC_STATEMENT_TYPE_OBJID in wintrust.h
	cryptAttributesArray.cAttr = 2;
	cryptAttributesArray.rgAttr = cryptAttribute;

	// Prepare SIGNER_SIGNATURE_INFO struct
	signerSignatureInfo.cbSize = sizeof(SIGNER_SIGNATURE_INFO);
	signerSignatureInfo.algidHash = CALG_SHA_256;
	signerSignatureInfo.dwAttrChoice = SIGNER_NO_ATTR;
	signerSignatureInfo.pAttrAuthcode = NULL;
	signerSignatureInfo.psAuthenticated = &cryptAttributesArray;
	signerSignatureInfo.psUnauthenticated = NULL;

	LPCSTR archivos[] = { "xhotas_usb.cat", "xhotas_vhid.cat" };
	for (char i = 0; i < 2; i++)
	{
		wszFileName = UTF8toWCHAR(archivos[i]);
		if (wszFileName == NULL) {
			wdi_warn("Unable to convert '%s' to UTF16", archivos[i]);
			goto out;
		}
		signerFileInfo.pwszFileName = wszFileName;

		// Sign file with cert
		hResult = pfSignerSignEx(0, &signerSubjectInfo, &signerCert, &signerSignatureInfo, NULL, NULL, NULL, NULL, &pSignerContext);
		if (hResult != S_OK) {
			wdi_warn("SignerSignEx failed: %s", hResult, winpki_error_str(hResult));
			free((void*)wszFileName);
			goto out;
		}
		r = TRUE;
		wdi_info("Successfully signed file '%s'", archivos[i]);
		free((void*)wszFileName);
	}

	// Clean up
out:
	/*
	 * Because we installed our certificate as a Root CA as well as a Trusted Publisher
	 * we *MUST* ensure that the private key is destroyed, so that it cannot be reused
	 * by an attacker to self sign a malicious applications.
	 */
	if ((pCertContext != NULL) && (DeletePrivateKey(pCertContext))) {
		wdi_info("Successfully deleted private key");
	}
	if (pSignerContext != NULL)
		pfSignerFreeSignerContext(pSignerContext);
	if (pCertContext != NULL)
		pfCertFreeCertificateContext(pCertContext);
	PF_FREE_LIBRARY(MSSign32);
	PF_FREE_LIBRARY(Crypt32);
	return r;
}