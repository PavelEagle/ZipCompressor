# ZipCompressor
Multithreading console application for block-by-block compression and decompression of files using **GzipStream**.

For compression, the source file is divided into blocks of the same size - 1 megabyte (can be changed DefaultByteBufferSize in ApplicationConstants). Each block is compressed and written to the output file independently of the rest of the blocks. 

Example of command line arguments:   
`ZipTest.exe compress/decompress [source filename] [resulted filename]`

Programm exit codes (to exit from application used Environment.Exit(int exitCode) method):  
0 - successful execution;  
1 - failed execution.

