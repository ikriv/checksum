Checksum, Copyright(C) 2014 Ivan Krivyakov.
Calculates and optionally validates file checksum.

USAGE: checksum algorithm file [expectedValue]

Supported algorithms: MD5, SHA1, SHA256, SHA384, SHA512

RETURN CODES:
1   - if specified arguments are invalid
2   - if an error occured during computation, such as unreadable file or out of memory
128 - if expected value is specified and the checksum does not match it
0   - in all other cases

Example:
    checksum md5 myfile.txt 123456789abcdef0deadbeeffeedface
