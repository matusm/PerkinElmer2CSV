# PerkinElmer2CSV
A simple command line tool for batch export of Perkin Elmer spectra data from \*.SP and \*.PRF files into CSV files that was created out of pure frustration coming from "TimeBase" Perkin Elmer software export capabilities.
Format-specific code (only) is based on Perkin Elmer import plugin for Matlab, since no public SP file (or "Perkin Elmer block data file") format specification is available. PRF files are simpler than SP, they employ Microsoft Compond File standard v3, and only the data structures within streams use proprietary layouts.

Currently, only units are saved as a header. However, all the other known data blocks are still being read, and one can include their data into CSVs with a couple of lines of code.
Tested on Perkin Elmer Frontier FTIR spectrometer data files.

File processing is multithreaded, however the processing core is not exactly memory-efficient. This is a result of development time being the top priority at the time being.

# Usage
The app decides what file provider to use based on file extension, so make sure it's correct. 
*For now, only SP and PRF files are supported. Other PE-block files can be de-facto supported, however I have no means to test it.*
The app checks the magic number of the file ("PEPE" in HEX) prior to processing.
The app never writes to input files.

The only required command line parameter is the target path. Path can be a directory or a file. Multiple targets are supported (passed as multiple CLI parameters).

Options:
 - recursive folder processing (include subfolders): -r

# Output
Invariat-culture CSVs with a header (column units). Output file name is simply the input file name with the original extension and ".csv" appended to it.
