#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;

namespace AltoLib
{
    /// <summary>
    /// RFC 4180 形式の CSV を MasterDataTable が扱うシンプルな形式へ変換する。
    /// セル内のコンマと改行は、それぞれ <comma> と <br> へ置き換える。
    /// </summary>
    public static class MasterDataCsvParser
    {
        public static string Parse(string csvText)
        {
            if (string.IsNullOrWhiteSpace(csvText))
            {
                throw new FormatException("Downloaded CSV is empty.");
            }

            var rows = ParseRows(csvText);
            if (rows.Count == 0 || IsEmptyRow(rows[0]))
            {
                throw new FormatException("CSV header is empty.");
            }

            int headerColumnCount = rows[0].Count;
            var parsedCsv = new StringBuilder();
            foreach (var row in rows)
            {
                while (row.Count < headerColumnCount)
                {
                    row.Add(string.Empty);
                }

                for (int i = 0; i < row.Count; ++i)
                {
                    if (i > 0) { parsedCsv.Append(','); }
                    parsedCsv.Append(MasterDataCsvCellCodec.Encode(row[i]));
                }
                parsedCsv.Append('\n');
            }
            return parsedCsv.ToString();
        }

        static List<List<string>> ParseRows(string csvText)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var cell = new StringBuilder();
            bool isQuoted = false;
            bool endedWithLineBreak = false;

            for (int i = 0; i < csvText.Length; ++i)
            {
                char character = csvText[i];
                endedWithLineBreak = false;

                if (isQuoted)
                {
                    if (character == '"')
                    {
                        bool isEscapedQuote = i + 1 < csvText.Length && csvText[i + 1] == '"';
                        if (isEscapedQuote)
                        {
                            cell.Append('"');
                            ++i;
                        }
                        else
                        {
                            isQuoted = false;
                        }
                    }
                    else if (character == '\r' || character == '\n')
                    {
                        if (character == '\r' && i + 1 < csvText.Length && csvText[i + 1] == '\n')
                        {
                            ++i;
                        }
                        cell.Append('\n');
                    }
                    else
                    {
                        cell.Append(character);
                    }
                    continue;
                }

                if (character == '"')
                {
                    if (cell.Length > 0)
                    {
                        throw new FormatException($"Unexpected quote at character { i }.");
                    }
                    isQuoted = true;
                }
                else if (character == ',')
                {
                    row.Add(cell.ToString());
                    cell.Clear();
                }
                else if (character == '\r' || character == '\n')
                {
                    if (character == '\r' && i + 1 < csvText.Length && csvText[i + 1] == '\n')
                    {
                        ++i;
                    }
                    row.Add(cell.ToString());
                    cell.Clear();
                    rows.Add(row);
                    row = new List<string>();
                    endedWithLineBreak = true;
                }
                else
                {
                    cell.Append(character);
                }
            }

            if (isQuoted)
            {
                throw new FormatException("CSV contains an unterminated quoted cell.");
            }

            if (!endedWithLineBreak || row.Count > 0 || cell.Length > 0)
            {
                row.Add(cell.ToString());
                rows.Add(row);
            }
            return rows;
        }

        static bool IsEmptyRow(List<string> row)
        {
            foreach (string cell in row)
            {
                if (!string.IsNullOrEmpty(cell)) { return false; }
            }
            return true;
        }
    }
}
#endif
