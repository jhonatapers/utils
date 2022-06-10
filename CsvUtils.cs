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

            columnSplitter = columnSplitter + "(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)";

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

                if (allLines.Length == 0)
                    return dt;

                string firstLine = Regex.Replace(allLines[0].Replace("\u001a", ""), "^(\n)", "");

                if (containHeader)
                {
                    getHeader(firstLine, dt, columnSplitter);
                }
                else
                {
                    getColumns(firstLine, dt, columnSplitter);

                    string[] row = Regex.Split(firstLine, columnSplitter);

                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < row.Length; i++)
                    {
                        dr[i] = row[i];
                    }
                    dt.Rows.Add(dr);
                }

                for (int i = 1; i < allLines.Length; i++)
                {
                    string line = Regex.Replace(allLines[i].Replace("\u001a", ""), "^(\n)", "");
                    if (!line.Equals(""))
                    {
                        string[] row = Regex.Split(line, columnSplitter);

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
                if (csv.Length <= currentPosition + jump)
                    break;
                
                if (csv[currentPosition] == '\"')
                {
                    isInQuotes = !isInQuotes;
                }
                else if (csv.Substring(currentPosition, jump).Equals(splitter) && !isInQuotes)
                {
                    csvLines.Add(csv.Substring(startPosition, (currentPosition) - startPosition));
                    startPosition = currentPosition + jump;
                }
            }

            csvLines.Add(csv.Substring(startPosition));

            return csvLines.ToArray();
        }


        private static void getColumns(String firstLine, DataTable dt, string splitter)
        {
            string[] firstRow = Regex.Split(firstLine, splitter);

            for (int i = 0; i < firstRow.Length; i++)
            {
                dt.Columns.Add(i.ToString());
            }
        }

        private static void getHeader(String firstLine, DataTable dt, string splitter)
        {
            while (true)
            {
                bool isHeader = false;
                string[] headers = Regex.Split(firstLine, splitter);
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

                    return;
                }
            }
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

    }
