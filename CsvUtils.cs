    public static class Csv
    {


        public enum lineSplitter
        {
            CRLF,
            LF
        }

        //
        // Summary: Converte .csv para DataTable
        // Parameters:
        //   strFilePath: Caminho do arquivo .csv
        //   containHeader: true = contain header false = not conteain header (cabecalho)
        //   splitter: separador de colunas
        //   lineSplitter:separador de linhas (normalmente \n\r)
        //   replace: vetor de Strings que deseja remover
        //   encoding: Encoding do texto
        // Returns:
        //     Retorna DataTable do arquivo .csv
        public static DataTable convertCSVtoDataTable(byte[] bFile, bool containHeader, string columnSplitter, lineSplitter lineSplitter, string[] replace, Encoding encoding)
        {
            Stopwatch timeSpend = new Stopwatch();
            timeSpend.Start();

            DataTable dt = new DataTable();
            try
            {
                string _lineSplitter = "";

                switch (lineSplitter)
                {
                    case utiles.lineSplitter.CRLF:
                        _lineSplitter = "\r\n";
                        break;
                    case utiles.lineSplitter.LF:
                        _lineSplitter = "\n";
                        break;
                    default:
                        _lineSplitter = "\r\n";
                        break;
                }

                string[] allLines = getLines(encoding.GetString(bFile), _lineSplitter);
                int pointer = 0;

                if (allLines.Length == 0)
                    return dt;

                if (containHeader)
                {
                    bool headerSet = false;

                    while (!headerSet && pointer < allLines.Length) 
                    {
                        headerSet = setHeader(allLines[pointer].Replace("\u001a", ""), dt, columnSplitter);
                        pointer++;
                    }
                }
                else
                {
                    setColumns(allLines[pointer].Replace("\u001a", ""), dt, columnSplitter);
                }

                for (int i = pointer; i < allLines.Length; i++)
                {
                    string line = allLines[i].Replace("\u001a", "");
                    if (!line.Equals(""))
                    {
                        string[] row = getColumns(line, columnSplitter);

                        DataRow dr = dt.NewRow();
                        for (int j = 0; j < row.Length; j++)
                        {
                            dr[j] = row[j].ReplaceAll(replace, "").Trim('"');
                        }

                        dt.Rows.Add(dr);
                    }
                }
            }
            catch (Exception reader)
            {
                throw reader;
            }

            timeSpend.Stop();
            Console.WriteLine(timeSpend.Elapsed.TotalSeconds.ToString());

            return dt;
        }

        private static String[] getLines(String csv, String splitter)
        {
            List<String> csvLines = new List<String>();

            int jump = splitter.Length;
            int startPosition = 0;
            bool isInQuotes = false;
            for (int currentPosition = 0; currentPosition < csv.Length; currentPosition++)
            {                
                if (csv.Length < currentPosition + jump)
                    break;
                
                if (csv[currentPosition] == '\"')
                {
                    isInQuotes = !isInQuotes;
                }
                else if (csv.Substring(currentPosition, jump).Equals(splitter) && !isInQuotes)
                {
                    csvLines.Add(csv.Substring(startPosition, (currentPosition) - startPosition));
                    startPosition = currentPosition + jump;
                    currentPosition += jump - 1;
                }
            }

            csvLines.Add(csv.Substring(startPosition));

            return csvLines.ToArray();
        }

        private static String[] getColumns(String csvLine, String splitter)
        {
            List<String> csvColumns = new List<String>();
            int startPosition = 0;
            int jump = splitter.Length;
            bool isInQuotes = false;
            for (int currentPosition = 0; currentPosition < csvLine.Length; currentPosition++)
            {
                if (csvLine.Length < currentPosition + jump)
                    break;

                if (csvLine[currentPosition] == '\"')
                {
                    isInQuotes = !isInQuotes;
                }
                else if (csvLine.Substring(currentPosition, jump).Equals(splitter) && !isInQuotes)
                {
                    if (csvLine[startPosition] == '\"' && csvLine[startPosition] == '\"')
                    {
                        csvColumns.Add(csvLine.Substring(startPosition + 1, ((currentPosition) - startPosition) - 2));
                    }
                    else
                    {
                        csvColumns.Add(csvLine.Substring(startPosition, (currentPosition) - startPosition));
                    }
                    startPosition = currentPosition + jump;
                    currentPosition += jump -1;
                }
            }

            if (csvLine.Substring(startPosition).Equals(splitter))
            {
                csvColumns.Add("");
            }
            else
            {
                csvColumns.Add(csvLine.Substring(startPosition));
            }

            return csvColumns.ToArray();
        }

        private static void setColumns(String firstLine, DataTable dt, string splitter)
        {
            string[] firstRow = getColumns(firstLine, splitter);

            for (int i = 0; i < firstRow.Length; i++)
            {
                dt.Columns.Add(i.ToString());
            }
        }

        private static bool setHeader(String firstLine, DataTable dt, string splitter)
        {
            bool isHeader = false;
            string[] headers = getColumns(firstLine, splitter);
            foreach (string header in headers)
            {
                if (header.Equals(""))
                {
                    isHeader = false;
                    break;
                }

                isHeader = true;
            }

            if (isHeader)
            {
                for (int i = 0; i < headers.Length; i++)
                {
                    dt.Columns.Add(i.ToString() + "_" + headers[i].Trim('"'));
                }
            }

            return isHeader;
        }

        public static string ReplaceAll(this string original, string[] toBeReplaced, string newValue)
        {
            if (string.IsNullOrEmpty(original) || toBeReplaced == null || toBeReplaced.Length <= 0) return original;
            if (newValue == null) newValue = string.Empty;
            foreach (string str in toBeReplaced)
                if (!string.IsNullOrEmpty(str))
                    original = original.Replace(str, newValue);
            return original;
        }

        public static void convertDataTableToCSV(DataTable dataTable, string strFilePath, string splitter, Encoding encoding)
        {
            StreamWriter sw = new StreamWriter(strFilePath, false, encoding);
            //headers    
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                sw.Write(dataTable.Columns[i]);
                if (i < dataTable.Columns.Count - 1)
                {
                    sw.Write(splitter);
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dataTable.Rows)
            {
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(splitter))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dataTable.Columns.Count - 1)
                    {
                        sw.Write(splitter);
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }

    }
