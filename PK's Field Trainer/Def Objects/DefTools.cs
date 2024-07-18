using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;

/// <summary>
/// This class contains all of the objects, subobjects, methods, and other tools that I created to help parse and deal with City of Heroes .def files
/// </summary>
namespace Philotic_Knight
{
    /// <summary>
    /// A class to hold all objects found in a Def file
    /// </summary>
    public class DefObject
    {

        /// <summary>
        /// Primary constructor, takes the name of the object
        /// </summary>
        /// <param name="Name"></param>
        public DefObject(string Name)
        {
            ObjectName = Name;
        }

        public string ObjectName { get; set; }
        public List<DefSubObject> Attributes = new List<DefSubObject>();
        public List<DefProperty> BaseProperties = new List<DefProperty>();
    }

    /// <summary>
    /// A class to hold all SubObjects of a DefObject
    /// </summary>
    public class DefSubObject
    {
        /// <summary>
        /// Primary constructor, takes the name of the SubObject
        /// </summary>
        /// <param name="Name"></param>
        public DefSubObject(string Name)
        {
            ObjectName = Name;
        }

        public string ObjectName { get; set; }
        public string ObjectType { get; set; } = "";
        public List<DefProperty> SubObjectsProperties = new List<DefProperty>();
    }

    /// <summary>
    /// A class to hold all Properties, either under a DefObject, or under a DefSubObject
    /// </summary>
    public class DefProperty
    {

        /// <summary>
        /// Primary constructor, takes the name and value of the property
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        public DefProperty(string Name, string Value)
        {
            PropertyName = Name;
            PropertyValue = Value;
        }

        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
    }

    /// <summary>
    /// A set of static methods to facilitate DefObject handling
    /// </summary>
    public static class DefMethods
    {

        /// <summary>
        /// Converts a standard CoH Message file into a Dictionary of string, string
        /// </summary>
        /// <param name="msFile"></param>
        /// <returns></returns>
        public static Dictionary<string, string> MSFileToDictionary(string msFile)
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            bool XMLSeparator = false;
            string XMLKey = "";
            StringBuilder sb = null;
            foreach (string line in File.ReadAllLines(msFile))
            {
                string workingLine = line.Trim();
                if (line == null || line == "" || line.StartsWith("#") || line.StartsWith("//")) continue; //skip empty lines

                //Determine the separator
                string separator = "";
                if (line.Contains(@""" """)) separator = @""" """;
                else if (line.Contains(",")) separator = ",";
                else if (line.Contains(":")) separator = ":";

                // Special case for XML tags
                if (line.Contains("<<"))
                {
                    //Start the XML
                    XMLSeparator = true;
                    sb = new StringBuilder();
                    string[] elements = line.Split(new string[] { separator }, StringSplitOptions.None);
                    if (elements.Length != 2)
                    {
                        //Error
                        continue;
                    }
                    XMLKey = elements[0].Trim();
                    sb.AppendLine(elements[1].Trim());
                }
                else if (line.Contains(">>"))
                {
                    //End the XML
                    XMLSeparator = false;
                    sb.AppendLine(line);
                    if (!retVal.ContainsKey(XMLKey)) retVal.Add(XMLKey, sb.ToString());
                    XMLKey = null;
                    sb = null;
                }
                else if (XMLSeparator)
                {
                    //Just add to the XML string
                    sb.AppendLine(line);
                    continue;
                }
                else
                {
                    //Standard items go here
                    if (separator == "")
                    {
                        continue;
                    }

                    string[] elements = line.Split(new string[] { separator }, StringSplitOptions.None);
                    if (elements.Length != 2)
                    {
                        continue;
                    }

                    //Make sure to take out quotation marks, as we don't really need them anymore
                    string key = IxQuotes(elements[0].Trim() + (separator == @""" """ ? @"""" : ""));
                    string value = IxQuotes((separator == @""" """ ? @"""" : "") + elements[1].Trim());

                    if (!retVal.ContainsKey(key)) retVal.Add(key, value);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns a list of DefObjects that contain a specific AttributeName
        /// </summary>
        /// <param name="AttributeName"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static List<DefObject> GetDefObjectsWithAttribute(string AttributeName, List<DefObject> objects)
        {
            List<DefObject> retVal = new List<DefObject>();
            foreach (DefObject @object in objects)
            {
                foreach (DefSubObject da in @object.Attributes)
                {
                    if (da.ObjectName == AttributeName && !retVal.Contains(@object)) retVal.Add(@object);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns a list of DefObjects that have the associated PropertyName
        /// </summary>
        /// <param name="PropertyName"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static List<DefObject> GetDefObjectsWithProperty(string PropertyName, List<DefObject> objects)
        {
            List<DefObject> retVal = new List<DefObject>();
            foreach (DefObject @object in objects)
            {
                foreach (DefProperty dp in @object.BaseProperties)
                {
                    if (dp.PropertyName == PropertyName && !retVal.Contains(@object)) retVal.Add(@object);
                }

                foreach (DefSubObject da in @object.Attributes)
                {
                    foreach (DefProperty dp in da.SubObjectsProperties)
                    {
                        if (dp.PropertyName == PropertyName && !retVal.Contains(@object)) retVal.Add(@object);
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        /// Returns a List of DefObjects that have the passed in ObjectName
        /// </summary>
        /// <param name="ObjectName"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static List<DefObject> GetDefObjectsWithName(string ObjectName, List<DefObject> objects)
        {
            List<DefObject> retVal = new List<DefObject>();
            foreach (DefObject @object in objects)
            {
                if (@object.ObjectName == ObjectName && !retVal.Contains(@object)) retVal.Add(@object);
            }
            return retVal;
        }


        /// <summary>
        /// Writes the passed in list of DefObjects to a Def file with the passed in FileName
        /// </summary>
        /// <param name="defObjects"></param>
        /// <param name="fileName"></param>
        private static void WriteDefObjectsToDefFile(List<DefObject> defObjects, string fileName)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    sw.WriteLine("//Generated by The Philotic Knight's (Westley Bennett's) Def File Translator");
                    sw.WriteLine();

                    foreach (DefObject dObject in defObjects)
                    {
                        sw.WriteLine(dObject.ObjectName);
                        sw.WriteLine("{");

                        //Determine the "best" spacing to use to line everything up
                        int longestLength = GetLongestPropertyNameLength(dObject.BaseProperties);

                        string DataToWrite = "";

                        //Write out the base properties
                        foreach (DefProperty dp in dObject.BaseProperties)
                        {
                            DataToWrite = '\t' + dp.PropertyName + new string(' ', longestLength - dp.PropertyName.Length)
                                + '\t' + dp.PropertyValue;
                            sw.WriteLine(DataToWrite);
                        }

                        //Now, write out the attributes and their properties
                        foreach (DefSubObject da in dObject.Attributes)
                        {
                            sw.WriteLine();
                            sw.WriteLine('\t' + da.ObjectName);
                            sw.WriteLine('\t' + "{");

                            longestLength = GetLongestPropertyNameLength(da.SubObjectsProperties);

                            foreach (DefProperty dp in da.SubObjectsProperties)
                            {
                                //Have to add a pipe and remove it later, because for some reason adding two tabs at the beginning of a line adds
                                //the string "18" to the beginning of the text for some reason - weird!
                                DataToWrite = "|" + '\t' + '\t' + dp.PropertyName + new string(' ', longestLength - dp.PropertyName.Length)
                                    + '\t' + dp.PropertyValue;
                                DataToWrite = DataToWrite.Substring(1);
                                sw.WriteLine(DataToWrite);
                            }

                            sw.WriteLine('\t' + "}");
                            sw.WriteLine();
                        }

                        sw.WriteLine("}");
                        sw.WriteLine();

                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error writing to Def file '" + fileName + "': " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Takes an input Def file and returns a list of DefObjects
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static List<DefObject> GetDefObjectsFromDefFile(string FileName)
        {
            int LineFailedAt = 0;
            try
            {
                List<DefObject> Objects = new List<DefObject>();
                DefObject CurrentObject = null;
                DefSubObject CurrentSubObject = null;
                string Extension = Path.GetExtension(FileName).ToUpper();

                string[] lines = File.ReadAllText(FileName).ToString().Trim().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                for (int i = 0; i < lines.Length; i++)
                {
                    LineFailedAt = i + 1;
                    string LineToUse = lines[i].Trim();
                    string NextLine = (i + 1) == lines.Length ? null : lines[i + 1].Trim();
                    //Ignore comments, blank lines, and open brackets
                    if ((LineToUse == "" && Extension != ".POWERSETS") || LineToUse.StartsWith("//") || LineToUse == "{") continue;
                    if (LineToUse == "" && Extension == ".POWERSETS" && CurrentSubObject == null) continue;

                    if (CurrentObject == null)
                    {
                        if (LineToUse == "}") continue; //Ignore if we're somehow ending an object when we have no object
                        if (NextLine != "{")
                        {
                            throw new Exception("Expected start of object, instead found '" + NextLine + " on line " + (i + 1));
                        }
                        //If there's a space in the object, then take the second element
                        try
                        {
                            CurrentObject = new DefObject(LineToUse.Contains(" ") ? LineToUse.Split(new string[] { " " }, StringSplitOptions.None)[1] : LineToUse);
                        }
                        catch
                        {
                            Debug.WriteLine("");
                        }

                        continue;
                    }
                    else if (LineToUse == "}")
                    {
                        //We must be either at the end of an Attribute or an Object
                        if (CurrentSubObject != null)
                        {
                            if (CurrentSubObject.ObjectType == "Power")
                            {
                                //Close the sub-object AND the object, since it's the end of a whole PowerSet
                                CurrentObject.Attributes.Add(CurrentSubObject);
                                CurrentSubObject = null;
                                Objects.Add(CurrentObject);
                                CurrentObject = null;
                                continue;
                            }
                            else
                            {
                                //Close the sub-object
                                CurrentObject.Attributes.Add(CurrentSubObject);
                                CurrentSubObject = null;
                                continue;
                            }
                        }
                        else
                        {
                            Objects.Add(CurrentObject);
                            CurrentObject = null;
                            continue;
                        }
                    }
                    else if (Extension == ".POWERSETS" && LineToUse.StartsWith("Powers"))
                    {
                        CurrentSubObject = new DefSubObject(LineToUse.Split(new string[] { "Powers " }, StringSplitOptions.None)[1].Trim());
                        //Add the first property artificially as the "Name"
                        DefProperty dp = new DefProperty("Name", LineToUse.Split('.')[2].Trim());
                        //Set the type to be used as a check later
                        CurrentSubObject.ObjectType = "Power";
                        CurrentSubObject.SubObjectsProperties.Add(dp);
                        continue;
                    }
                    else if (Extension == ".POWERSETS" && LineToUse == "")
                    {
                        //We must be either at the end of an Attribute or an Object
                        if (CurrentSubObject != null)
                        {
                            CurrentObject.Attributes.Add(CurrentSubObject);
                            CurrentSubObject = null;
                            continue;
                        }
                        else throw new Exception("Unhandled empty line in powersets file.");
                    }
                    else if (NextLine == "{")
                    {
                        //This must be a new attribute
                        CurrentSubObject = new DefSubObject(lines[i].Trim());
                        continue;
                    }
                    else
                    {
                        //This is a a new property, create it, Split the string at the first space
                        DefProperty Property = null;

                        //Have to handle the chance for only tabs rather than spaces
                        if (LineToUse.Contains("\t"))
                        {
                            //Assume tab character
                            string[] elements = LineToUse.Split('\t');
                            if (elements.Length == 1)
                            {
                                //Assume a new  Attribute
                                CurrentSubObject = new DefSubObject(elements[0].Trim());
                                //Add the first property artificially as the "Name"
                                DefProperty dp = new DefProperty("Name", elements[0].Trim());
                                CurrentSubObject.SubObjectsProperties.Add(dp);
                                continue;
                            }

                            //Otherwise, if it's a tab, add it as a DefProperty
                            Property = new DefProperty(LineToUse.Substring(0, LineToUse.IndexOf('\t')),
                            LineToUse.Substring(LineToUse.IndexOf('\t')).Trim());
                        }
                        else
                        {
                            try
                            {
                                //Assume space character
                                string[] elements = LineToUse.Split(' ');
                                if (elements.Length == 1)
                                {
                                    //Assume a new  Attribute
                                    CurrentSubObject = new DefSubObject(elements[0].Trim());
                                    //Add the first property artificially as the "Name"
                                    DefProperty dp = new DefProperty("Name", elements[0].Trim());
                                    CurrentSubObject.SubObjectsProperties.Add(dp);
                                    continue;
                                }

                                Property = new DefProperty(LineToUse.Substring(0, LineToUse.IndexOf(" ")),
                                    LineToUse.Substring(LineToUse.IndexOf(" ")).Trim());
                            }
                            catch
                            {
                                Debug.WriteLine("");
                            }
                        }

                        //If there's an attribute, it must belong to that
                        if (CurrentSubObject != null)
                        {
                            CurrentSubObject.SubObjectsProperties.Add(Property);
                            continue;
                        }
                        else
                        {
                            //Otherwise, it's an object
                            CurrentObject.BaseProperties.Add(Property);
                            continue;
                        }
                    }
                }
                return Objects;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in file '" + FileName + "' at line " + LineFailedAt + ": " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Takes a list of DefObjects, and converts it into a DataSet
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static DataSet GetDataSetFromDefObjects(List<DefObject> objects)
        {
            try
            {
                DataSet ds = new DataSet();
                int x = 0;
                foreach (DefObject dObject in objects)
                {
                    x++;
                    Debug.WriteLine("Handling Object " + x + " of " + objects.Count);

                    //Add the main object as a new table
                    Debug.WriteLine("Adding Table '" + dObject.ObjectName + "'");
                    if (!ds.Tables.Contains(dObject.ObjectName)) ds.Tables.Add(dObject.ObjectName);

                    //Add all of its base properties as new rows and columns
                    foreach (DefProperty dp in dObject.BaseProperties)
                    {
                        if (!ds.Tables[dObject.ObjectName].Columns.Contains(dp.PropertyName))
                            ds.Tables[dObject.ObjectName].Columns.Add(dp.PropertyName);
                    }
                    //Make the first new row of properties
                    DataRow dr = ds.Tables[dObject.ObjectName].NewRow();

                    //Populate the properties
                    foreach (DefProperty dp in dObject.BaseProperties)
                    {
                        //For duplicate properties, create duplicate rows
                        if (!IsNullOrEmpty(dr[dp.PropertyName]) && dp.PropertyValue != dr[dp.PropertyName].ToString())
                        {
                            DataRow drProp = ds.Tables[dObject.ObjectName].NewRow();
                            foreach (DataColumn dc in ds.Tables[dObject.ObjectName].Columns)
                            {
                                drProp[dc.ColumnName] = dr[dc.ColumnName];
                            }
                            drProp[dp.PropertyName] = dp.PropertyValue;
                            ds.Tables[dObject.ObjectName].Rows.Add(drProp);
                            continue;
                        }
                        else
                        {
                            //For singular properties, just set the property
                            dr[dp.PropertyName] = dp.PropertyValue;
                        }
                    }
                    ds.Tables[dObject.ObjectName].Rows.Add(dr);

                    //Now, do the same for each attribute of that Object
                    foreach (DefSubObject da in dObject.Attributes)
                    {

                        //Add each Attribute as its own table
                        if (!ds.Tables.Contains(da.ObjectName))
                        {
                            Debug.WriteLine("Adding Table '" + da.ObjectName + "'");
                            ds.Tables.Add(da.ObjectName);
                            ds.Tables[da.ObjectName].Columns.Add("ParentName"); //Add the parent object as a column
                        }

                        //Add all of the missing "columns" to the Object's datatable for this attribute
                        foreach (DefProperty dp in da.SubObjectsProperties)
                        {
                            if (!ds.Tables[da.ObjectName].Columns.Contains(dp.PropertyName))
                                ds.Tables[da.ObjectName].Columns.Add(dp.PropertyName);
                        }

                        //Add all of the attribute's properties as a new row of data
                        dr = ds.Tables[da.ObjectName].NewRow();

                        //Populate them, starting with the name of the parent, assuming it's the first value
                        dr["ParentName"] = dObject.BaseProperties[0].PropertyValue;
                        foreach (DefProperty dp in da.SubObjectsProperties)
                        {
                            //For duplicate properties, create duplicate rows
                            if (!IsNullOrEmpty(dr[dp.PropertyName]) && dp.PropertyValue != dr[dp.PropertyName].ToString())
                            {
                                DataRow drProp = ds.Tables[da.ObjectName].NewRow();
                                foreach (DataColumn dc in ds.Tables[da.ObjectName].Columns)
                                {
                                    drProp[dc.ColumnName] = dr[dc.ColumnName];
                                }
                                drProp[dp.PropertyName] = dp.PropertyValue;
                                ds.Tables[da.ObjectName].Rows.Add(drProp);
                                continue;
                            }
                            else
                            {
                                //For singular properties, just set the property
                                dr[dp.PropertyName] = dp.PropertyValue;
                            }
                        }
                        ds.Tables[da.ObjectName].Rows.Add(dr);
                    }
                }

                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to convert DefObjects into DataSet.", ex);
            }
        }

        /// <summary>
        /// Checks whether a passed in object is null or an empty string
        /// </summary>
        /// <param name="possibleString"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(object possibleString)
        {
            if (possibleString == null) return true;
            if (possibleString == DBNull.Value) return true;
            if (possibleString.ToString().Trim() == "") return true;
            return false;
        }

        /// <summary>
        /// Checks whether or not a specific worksheet name exists in a collection of WorkSheets
        /// </summary>
        /// <param name="WorkSheetName"></param>
        /// <param name="WorkSheets"></param>
        /// <returns></returns>
        private static bool WorkSheetExists(string WorkSheetName, ExcelWorksheets WorkSheets)
        {
            try
            {
                foreach (ExcelWorksheet ws in WorkSheets)
                {
                    if (ws.Name == WorkSheetName) return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to detmine existence of '" + WorkSheetName + "' in WorkSheets.", ex);
            }
        }

        /// <summary>
        /// Converts an appropriately formatted Def file into an Excel file
        /// </summary>
        /// <param name="inputFileName">Input Def File</param>
        /// <param name="outputFileName">Output Excel file</param>
        /// <returns></returns>
        public static bool ConvertDefFileToExcelFile(string inputFileName, string outputFileName)
        {
            try
            {
                List<DefObject> dObjects = GetDefObjectsFromDefFile(inputFileName);
                if (dObjects == null || dObjects.Count == 0) throw new Exception("No Def Objects found in file '" + inputFileName + "'");
                using (DataSet ds = GetDataSetFromDefObjects(dObjects))
                {
                    WriteDataSetToExcelFile(ds, outputFileName);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to convert file '" + inputFileName + "' to file '" + outputFileName + "'", ex);
            }
        }

        /// <summary>
        /// Converts an appropriately formatted Excel file into a Def file
        /// </summary>
        /// <param name="inputFileName">Input Excel File</param>
        /// <param name="outputFileName">Output Def File</param>
        /// <returns></returns>
        public static bool ConvertExcelFileToDefFile(string inputFileName, string outputFileName)
        {
            try
            {
                List<DefObject> dObjects = GetDefObjectsFromExcelFile(inputFileName);
                if (dObjects == null || dObjects.Count == 0) throw new Exception("No Def Objects found in file '" + inputFileName + "'");
                using (DataSet ds = GetDataSetFromDefObjects(dObjects))
                {
                    WriteDefObjectsToDefFile(dObjects, outputFileName);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to convert file '" + inputFileName + "' to file '" + outputFileName + "'", ex);
            }
        }

        /// <summary>
        /// Writes a DataSet to an Excel file
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="fileName"></param>
        public static void WriteDataSetToExcelFile(DataSet ds, string fileName)
        {
            try
            {
                ds = PreventDBNull(ds);
                using (ExcelPackage p = new ExcelPackage(new FileInfo(fileName)))
                {
                    foreach (DataTable dt in ds.Tables)
                    {
                        if (dt == null || dt.Rows.Count == 0) continue;
                        //Make sure we have unique worksheet names
                        string WorkSheetName = dt.TableName;
                        string FirstWorkSheetName = WorkSheetName;
                        int WorkSheetNumber = 0;
                        do
                        {
                            if (WorkSheetName.Length > 31) WorkSheetName = WorkSheetName.Substring(0, 31);
                            if (WorkSheetExists(WorkSheetName, p.Workbook.Worksheets))
                            {
                                WorkSheetNumber++;
                                WorkSheetName = WorkSheetName.Substring(0, WorkSheetName.Length - WorkSheetNumber.ToString().Length) +
                                    WorkSheetNumber.ToString();
                            }
                        } while (WorkSheetExists(WorkSheetName, p.Workbook.Worksheets));

                        if (dt.Rows.Count > 1000000)
                        {
                            int SheetNum = 0;
                            int RowNum = 0;
                            DataTable dtCurrent = dt.Clone();
                            foreach (DataRow dr in dt.Rows)
                            {
                                RowNum++;
                                //Reset every million rows to defeat Excel's limitations
                                if (RowNum > 1000000)
                                {
                                    //Reset everything
                                    SheetNum++;
                                    RowNum = 0;
                                    //Load the current data into the sheet
                                    WorkSheetName = FirstWorkSheetName + (SheetNum == 0 ? "" : SheetNum.ToString());
                                    
                                    //Check if the worksheet exists, if it doesn't add it
                                    try
                                    {
                                        //Self referential reference just to see if it exists
                                        WorkSheetName = p.Workbook.Worksheets[WorkSheetName].Name;
                                    }
                                    catch
                                    {
                                        //If it doesn't, add it
                                        WorkSheetName = FirstWorkSheetName + (SheetNum == 0 ? "" : SheetNum.ToString());
                                        p.Workbook.Worksheets.Add(WorkSheetName);
                                    }

                                    p.Workbook.Worksheets[WorkSheetName].Cells["A1"].LoadFromDataTable(dtCurrent, true);
                                    //Empty the datatable and start anew
                                    dtCurrent = dt.Clone();
                                }

                                dtCurrent.ImportRow(dr);
                            }
                        }
                        else
                        {
                            //Check if the worksheet exists, if it doesn't add it
                            try
                            {
                                //Self referential reference just to see if it exists
                                WorkSheetName = p.Workbook.Worksheets[WorkSheetName].Name;
                            }
                            catch (Exception ex)
                            {
                                //If it doesn't, add it
                                p.Workbook.Worksheets.Add(WorkSheetName);
                            }

                            p.Workbook.Worksheets[WorkSheetName].Cells["A1"].LoadFromDataTable(dt, true);
                        }
                    }
                    p.Save();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to write DataSet to File '" + fileName + "'", ex);
            }
        }

        /// <summary>
        /// Writes the passed in DataSet into a Def file
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="fileName"></param>
        public static void WriteDataSetToDefFile(DataSet ds, string fileName)
        {
            try
            {
                ds = PreventDBNull(ds);
                List<DefObject> dObjects = GetDefObjectsFromDataSet(ds);
                WriteDefObjectsToDefFile(dObjects, fileName);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to write DataSet to File '" + fileName + "'", ex);
            }
        }

        /// <summary>
        /// Gets a List of DefObjects from a passed in DataSet
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static List<DefObject> GetDefObjectsFromDataSet(DataSet ds)
        {
            try
            {
                if (ds == null || ds.Tables.Count == 0) throw new Exception("No data found in dataset.");

                List<DefObject> returnValue = new List<DefObject>();

                foreach (DataTable dt in ds.Tables)
                {
                    DefObject dObject = new DefObject(dt.Rows[0]["ObjectName"].ToString());
                    //Get the base properties first
                    foreach (DataColumn dc in dt.Columns)
                    {
                        if (dt.Rows[0][dc.ColumnName].ToString().Trim() != "") dObject.BaseProperties.Add(
                            new DefProperty(dc.ColumnName, dt.Rows[0][dc.ColumnName].ToString().Trim()));
                    }

                    //Get the attributes and their properties
                    int RowNum = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        //Skip first row, since we've handled it above
                        if (RowNum == 0)
                        {
                            RowNum++;
                            continue;
                        }

                        DefSubObject da = new DefSubObject(dr["ObjectName"].ToString());

                        //Get the Attribute's properties
                        foreach (DataColumn dc in dt.Columns)
                        {
                            if (dr[dc.ColumnName].ToString().Trim() != "") da.SubObjectsProperties.Add(
                                new DefProperty(dc.ColumnName, dr[dc.ColumnName].ToString().Trim()));
                        }
                        dObject.Attributes.Add(da);
                    }
                    returnValue.Add(dObject);
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get DefObjects from DataSet.", ex);
            }
        }

        /// <summary>
        /// Prevents data in a DataSet from ever being "DBNull". Prevents errors when exporting to Excel
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static DataSet PreventDBNull(DataSet dataSet)
        {
            try
            {
                foreach (DataTable dataTable in dataSet.Tables)
                    foreach (DataRow dataRow in dataTable.Rows)
                        foreach (DataColumn dataColumn in dataTable.Columns)
                            if (dataRow.IsNull(dataColumn))
                            {
                                if (dataColumn.DataType.IsValueType) dataRow[dataColumn] = Activator.CreateInstance(dataColumn.DataType);
                                else if (dataColumn.DataType == typeof(bool)) dataRow[dataColumn] = false;
                                else if (dataColumn.DataType == typeof(Guid)) dataRow[dataColumn] = Guid.Empty;
                                else if (dataColumn.DataType == typeof(string)) dataRow[dataColumn] = string.Empty;
                                else if (dataColumn.DataType == typeof(DateTime)) dataRow[dataColumn] = DateTime.MaxValue;
                                else if (dataColumn.DataType == typeof(int) || dataColumn.DataType == typeof(byte) || dataColumn.DataType == typeof(short) || dataColumn.DataType == typeof(long) || dataColumn.DataType == typeof(float) || dataColumn.DataType == typeof(double)) dataRow[dataColumn] = 0;
                                else
                                {
                                    dataRow[dataColumn] = null;
                                }
                            }

                return dataSet;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to clean DataSet of null values.", ex);
            }
        }

        /// <summary>
        /// Gets the longest Property name in a list of DefProperties, and returns the length
        /// </summary>
        /// <param name="baseProperties"></param>
        /// <returns></returns>
        public static int GetLongestPropertyNameLength(List<DefProperty> baseProperties)
        {
            try
            {
                int longestLength = 0;

                foreach (DefProperty dp in baseProperties)
                {
                    if (dp.PropertyName.Length > longestLength) longestLength = dp.PropertyName.Length;
                }

                return longestLength;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get longest length of DefProperty Names.", ex);
            }
        }

        /// <summary>
        /// Gets a list of DefObjects from an ExcelFile
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static List<DefObject> GetDefObjectsFromExcelFile(string fileName)
        {
            try
            {
                List<DefObject> DefObjects = null;
                using (DataSet ds = GetDataSetFromExcelFile(fileName))
                {
                    if (ds == null || ds.Tables.Count == 0) return DefObjects;
                    DefObjects = new List<DefObject>();
                    foreach (DataTable dt in ds.Tables)
                    {
                        //Start creating the object and its properties
                        DefObject dObject = new DefObject(dt.TableName);
                        //Make sure that we're working with our custom file type
                        if (!dt.Columns.Contains("ObjectName") ||
                            dt.Rows[0]["ObjectName"].ToString().Trim() == "" ||
                            dt.Rows[0]["ObjectName"].ToString() != dObject.ObjectName) throw new Exception(
                                "Could not find object name for DefObject in file.");

                        //Add all of the base properties
                        foreach (DataColumn dc in dt.Columns)
                        {
                            if (dc.ColumnName == "ObjectName") continue;
                            DefProperty dp = new DefProperty(dc.ColumnName, dt.Rows[0][dc.ColumnName].ToString().Trim());
                            if (dp.PropertyValue.ToString() != "") dObject.BaseProperties.Add(dp);
                        }

                        //Add all of the Attributes and their properties
                        int iRowNumber = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (iRowNumber == 0)
                            {
                                iRowNumber++;
                                continue;
                            }

                            //Create the DefAttribute objects
                            DefSubObject da = new DefSubObject(dr["ObjectName"].ToString());

                            //Add its properties
                            foreach (DataColumn dc in dt.Columns)
                            {
                                if (dc.ColumnName == "ObjectName") continue;
                                DefProperty dp = new DefProperty(dc.ColumnName, dr[dc.ColumnName].ToString().Trim());
                                if (dp.PropertyValue.ToString() != "") da.SubObjectsProperties.Add(dp);
                            }
                            dObject.Attributes.Add(da);
                        }
                        DefObjects.Add(dObject);
                    }
                }

                return DefObjects;
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting DefObjects from file '" + fileName + "': " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Returns a populated DataSet from an Excel spreadsheet
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static DataSet GetDataSetFromExcelFile(string fileName)
        {
            try
            {
                using (DataSet ds = new DataSet())
                {
                    using (ExcelPackage p = new ExcelPackage(new FileInfo(fileName)))
                    {
                        if (p.Workbook == null || p.Workbook.Worksheets == null || p.Workbook.Worksheets.Count == 0) return null;

                        foreach (ExcelWorksheet ws in p.Workbook.Worksheets)
                        {
                            //Rely on the A2 cell for the primary ObjectName
                            DataTable dt = new DataTable(ws.Cells["A2"].Value.ToString());

                            //Add all of the columns
                            foreach (ExcelRangeBase c in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                            {
                                if (!dt.Columns.Contains(c.Value.ToString())) dt.Columns.Add(c.Value.ToString());
                            }

                            //Add the rest of the actual data into the DataTable
                            for (int rowNum = 2; rowNum <= ws.Dimension.End.Row; rowNum++)
                            {
                                DataRow newRow = dt.NewRow();

                                //loop all cells in the row
                                for (int colNum = 1; colNum <= ws.Dimension.End.Column; colNum++)
                                {
                                    string colName = ws.Cells[1, colNum, 1, colNum].Value.ToString();
                                    string value = ws.Cells[rowNum, colNum, rowNum, colNum].Value == null ? "" :
                                        ws.Cells[rowNum, colNum, rowNum, colNum].Value.ToString().Trim();
                                    newRow[colName] = value;
                                }

                                dt.Rows.Add(newRow);
                            }
                            ds.Tables.Add(dt);
                        }
                    }
                    return ds;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load DataSet from file '" + fileName + "'", ex);
            }
        }

        /// <summary>
        /// Ix-nays the otes-quays from an ing-stray
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string IxQuotes(string input)
        {
            return input.Replace(@"""", "");
        }
    }


}
