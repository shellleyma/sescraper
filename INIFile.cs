using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace SEScraper
{

    /// <summary>
    /// Contains native Win32 API methods found in Kernel32. 
    /// </summary>
    public static class Kernel32
    {
        /// <summary>
        /// Retrieves a string from the specified section in an initialization file.
        /// </summary>
        /// <param name="lpAppName">The name of the section containing the key name. If this parameter is NULL, the GetPrivateProfileString function copies all section names in the file to the supplied buffer.</param>
        /// <param name="lpKeyName">The name of the key whose associated string is to be retrieved. If this parameter is NULL, all key names in the section specified by the lpAppName parameter are copied to the buffer specified by the lpReturnedString parameter.</param>
        /// <param name="lpDefault">A default string. If the lpKeyName key cannot be found in the initialization file, GetPrivateProfileString copies the default string to the lpReturnedString buffer. If this parameter is NULL, the default is an empty string, "".
        ///                         <para>Avoid specifying a default string with trailing blank characters. The function inserts a null character in the lpReturnedString buffer to strip any trailing blanks.</para></param>
        /// <param name="lpReturnString">A pointer to the buffer that receives the retrieved string.</param>
        /// <param name="nSize">The size of the buffer pointed to by the lpReturnedString parameter, in characters.</param>
        /// <param name="lpFilename">The name of the initialization file. If this parameter does not contain a full path to the file, the system searches for the file in the Windows directory.</param>
        /// <returns>The return value is the number of characters copied to the buffer, not including the terminating null character.</returns>
        [DllImport("Kernel32.dll", EntryPoint = "GetPrivateProfileStringW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetPrivateProfileString(string lpAppName,
                                                         string lpKeyName,
                                                         string lpDefault,
                                                         [In, Out] char[] lpReturnString,
                                                         int nSize,
                                                         string lpFilename);

    }


    /// <summary>
    /// Represents the <i>common</i> INI file format consisting of parameter-value pairs grouped by sections.
    /// </summary>
    public class INIFile
    {
        #region Nested Types
        public enum CaseSensitiviy
        {
            CaseInsensitive,
            CaseSensitive
        }
        #endregion

        #region Properties
        /// <summary>
        /// Provides the string value of parameter belonging to the specified section. The section and parameter must exist.
        /// </summary>
        /// <param name="sectionName">The name of the section the parameter belongs to.</param>
        /// <param name="parameterName">The name of the parameter containing the value.</param>
        /// <exception cref="System.ArgumentException">Thrown if either the section or parameter name do not exist.</exception>
        /// <returns>The value of the parameter.</returns>
        public string this[string sectionName, string parameterName]
        {
            get
            {
                if (_sections.ContainsKey(sectionName))
                {
                    if (_sections[sectionName].ContainsKey(parameterName))
                        return _sections[sectionName][parameterName];
                    else
                        throw new ArgumentException("ErrorBadIniFile");
                }
                else
                    throw new ArgumentException("ErrorBadIniFile");
            }
        }

        /// <summary>
        /// Specifies what case sensitivity mode is being used when looking up section and parameter names.
        /// </summary>
        public CaseSensitiviy CaseSensitivity
        {
            get { return _caseSensitivity; }
        }
        #endregion

        #region Events
        #endregion

        #region Constructors
        /// <summary>
        /// Loads the specified INI file. By default, all searches for section and parameter names will be case sensitive.
        /// </summary>
        /// <param name="filename">The file name and path to the INI file. If this parameter does not contain a full path to the file, the method will search the Windows directory.</param>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if filename could not be found and doesn't exist in the Window directory.</exception>
        public INIFile(string filename) :
            this(filename, CaseSensitiviy.CaseSensitive)
        {

        }

        /// <summary>
        /// Loads the specified INI file.
        /// </summary>
        /// <param name="filename">The file name and path to the INI file. If this parameter does not contain a full path to the file, the method will search the Windows directory.</param>
        /// <param name="sensitivity">Specifies if letter case should be accounted for when looking up section and parameter names.</param>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if filename could not be found and doesn't exist in the Window directory.</exception>
        public INIFile(string filename, CaseSensitiviy sensitivity)
        {
            // make sure the file exists, directly, or in the Windows directory
            if (!File.Exists(filename))
                if (!File.Exists(Path.Combine(Environment.ExpandEnvironmentVariables("%WinDir%"), filename)))
                    throw new FileNotFoundException();

            // save local state
            _caseSensitivity = sensitivity;
            _readBufferSize = _readBufferDefaultSize;
            _readBuffer = new char[_readBufferSize];

            // read all section names
            string[] raw = GetPrivateProfileString(null, null, null, filename);

            // the data is stored in a dictionary, we must tell it if case sensitivity is important
            if (_caseSensitivity == CaseSensitiviy.CaseInsensitive)
                _sections = new Dictionary<string, IDictionary<string, string>>(StringComparer.CurrentCultureIgnoreCase);
            else
                _sections = new Dictionary<string, IDictionary<string, string>>();

            // now, read in all parameters and values for each section name we located
            foreach (string s in raw)
                ReadSection(filename, s, _sections);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Checks to see if a given section exists in the Ini file.
        /// </summary>
        /// <param name="sectionName">The section name to check for.</param>
        /// <returns>True if the section exists, false if not.</returns>
        public bool DoesSectionExist(string sectionName)
        {
            return _sections.ContainsKey(sectionName);
        }

        /// <summary>
        /// Checks to see if a given parameter exists for a section.
        /// </summary>
        /// <param name="sectionName">The name of the section containing the parameter.</param>
        /// <param name="parameterName">The name of the parameter to check for.</param>
        /// <returns>True if the parameter exists, false if not.</returns>
        public bool DoesParameterExist(string sectionName, string parameterName)
        {
            if (DoesSectionExist(sectionName))
                return _sections[sectionName].ContainsKey(parameterName);
            else
                return false;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Reads all parameters and values for a given section within the Ini file.
        /// </summary>
        /// <param name="inifile">The ini file to read from.</param>
        /// <param name="sectionName">The name of the section to read.</param>
        /// <param name="storage">A dictionary containing an entry for the section that will be populated with the resulting parameters/values.</param>
        private void ReadSection(string inifile, string sectionName, IDictionary<string, IDictionary<string, string>> storage)
        {
            // create the parameter/value dictionary for the section entry
            // it is assumed that the callers logic created/validated storage[sectionName] -- we will not check again here
            if (_caseSensitivity == CaseSensitiviy.CaseInsensitive)
                storage[sectionName] = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            else
                storage[sectionName] = new Dictionary<string, string>();

            string[] raw = GetPrivateProfileString(sectionName, null, null, inifile);

            // now read in the values for the parameters that we found
            foreach (string s in raw)
                ReadParameter(inifile, sectionName, s, storage[sectionName]);
        }

        /// <summary>
        /// Reads the value of a parameter in a given section within the Ini file.
        /// </summary>
        /// <param name="inifile">The ini file to read from.</param>
        /// <param name="sectionName">The name of the section containing the parameter.</param>
        /// <param name="parameterName">The name of the parameter to read.</param>
        /// <param name="storage">The dictionary that the parameter value will be written to.</param>
        /// <exception cref="System.ArgumentException">Thrown if more than 1 value is associated with a single parameter.</exception>
        private void ReadParameter(string inifile, string sectionName, string parameterName, IDictionary<string, string> storage)
        {
            // get the param value
            string[] raw = GetPrivateProfileString(sectionName, parameterName, null, inifile);

            // store the resulting value, if more than 1 value comes back for the parameter throw an exception, not sure how to parse that scenario
            // if no values come back just use an empty string by default
            if (raw.Length > 1)
                throw new ArgumentException("ErrorBadIniFile");
            else if (raw.Length == 0)
                storage[parameterName] = "";
            else
                storage[parameterName] = raw[0];
        }

        /// <summary>
        /// Wrapper around the Win32 function GetPrivateProfileString to make it nicer and more robust.
        /// </summary>
        /// <param name="appName">The name of the section containing the key name. If this parameter is null, all section names will be returned.</param>
        /// <param name="keyName">The name of the key whose associated string is to be retrieved. If this parameter is null, all key names in the section will be returned.</param>
        /// <param name="defaultName">A default string. If the keyName key cannot be found in the initialization file, GetPrivateProfileString copies the default string to the 
        ///                           returned value. If this parameter is null, the default is an empty string, "".</param>
        /// <param name="inifile">The path and file name of the INI file. If this parameter does not contain a full path to the file, the system searches for the file in the Windows directory.</param>
        /// <returns>An array of string containing either section names, parameter names, or values corresponding to appName and keyName.</returns>
        /// <exception cref="FileLoadException"></exception>
        private string[] GetPrivateProfileString(string appName, string keyName, string defaultName, string inifile)
        {
            // the file inifile should exist as it's validated in the class constructor, however GetPrivateProfileString does not lock the file hence it could be locked or deleted by
            // another process between calls to this method

            // used to tell if we successfully read all requested data from the file 
            // from MSDN:
            //  If neither lpAppName nor lpKeyName is NULL and the supplied destination buffer is too small to hold the requested string, the string is truncated and followed by a null character, and the return value is equal to nSize minus one.
            //  If either lpAppName or lpKeyName is NULL and the supplied destination buffer is too small to hold all the strings, the last string is truncated and followed by two null characters. In this case, the return value is equal to nSize minus two.
            int sizeReadOffset = (appName != null && keyName != null) ? 1 : 2;

            // try and read the entire amount of data from the ini file
            int sizeRead = Kernel32.GetPrivateProfileString(appName, keyName, defaultName, _readBuffer, _readBufferSize, inifile);

            // if unable to read all of the data because the buffer is to small, increase the size and try again, but only allow memory to increase up to _readBufferMaxSize
            while ((sizeRead >= _readBufferSize - sizeReadOffset) && (_readBufferSize < _readBufferMaxSize))
            {
                _readBufferSize += 512;
                _readBuffer = new char[_readBufferSize];

                sizeRead = Kernel32.GetPrivateProfileString(appName, keyName, defaultName, _readBuffer, _readBufferSize, inifile);
            }

            // if memory consumption exceeded _readBufferMaxSize then we have a problem
            if (_readBufferSize >= _readBufferMaxSize)
                throw new FileLoadException("ErrorBadIniLoad");

            // the buffer returned from GetPrivateProfileString will be null terminated C-strings followed by a double null at the end - so split the strings on the nulls
            char[] sep = new char[1];
            sep[0] = '\0';

            string s = new string(_readBuffer, 0, sizeRead);

            string[] result = s.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            return result;
        }
        #endregion

        #region Fields
        /// <summary>
        /// Stores the sections from the Ini file. The section name is the key. The value is another dictionary containing the parameter values, keyed on the parameter name.
        /// </summary>
        public IDictionary<string, IDictionary<string, string>> _sections;

        /// <summary>
        /// Determines if case is ignored or not when referencing section and parameter names.
        /// </summary>
        private CaseSensitiviy _caseSensitivity;

        /// <summary>
        /// Holds the information from the last read of the ini file. The buffer is overwritten with sections, parameters, or values on each call to GetPrivateProfileString.
        /// </summary>
        private char[] _readBuffer;

        /// <summary>
        /// The size, in bytes, of _readBuffer.
        /// </summary>
        private int _readBufferSize;

        /// <summary>
        /// The starting size of _readBuffer, in bytes.
        /// </summary>
        private const int _readBufferDefaultSize = 1024;

        /// <summary>
        /// The maximum size _readBuffer is allowed to grow to in order to hold all information obtained from GetPrivateProfileString.
        /// </summary>
        private const int _readBufferMaxSize = 4096;
        #endregion
    }


    public static class tt
    {
        public static IDictionary<T, U> Clone<T, U>( IDictionary<T, U> dict)
        {
            IDictionary<T, U> dict2 = new Dictionary<T, U>();
            foreach (KeyValuePair<T, U> kvp in dict)
            {
                dict2.Add(kvp.Key, kvp.Value);
            }
            return dict2;
        }
    }
}
