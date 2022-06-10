    public static class Csv
    {
        private static String[] CsvSplitter(String csv, String splitter)
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
    }
