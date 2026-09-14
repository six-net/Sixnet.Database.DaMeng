using System.Collections.Generic;
using System;

using Sixnet.Development.Data.Field.Formatting;
using Sixnet.Exceptions;

namespace Sixnet.Database.DaMeng
{
    /// <summary>
    /// Default default field formatter for DaMeng
    /// </summary>
    public class SixnetDaMengDefaultFieldFormatter : ISixnetFieldFormatter
    {
        public string Format(SixnetFormatFieldContext context)
        {
            var formatOption = context.FormatSetting;
            var formatedFieldName = context.FieldName;
            var parameterString = formatOption.Parameter?.ToString();

            formatedFieldName = formatOption.Name switch
            {
                SixnetFieldFormatterNames.TO_STRING => $"CAST({formatedFieldName} AS VARCHAR({(string.IsNullOrWhiteSpace(parameterString) ? "4000" : parameterString)}))",
                SixnetFieldFormatterNames.DISTINCT => $"DISTINCT {formatedFieldName}",
                SixnetFieldFormatterNames.IS_NULL => $"{formatedFieldName} IS NULL",
                SixnetFieldFormatterNames.NOT_NULL => $"{formatedFieldName} IS NOT NULL",
                SixnetFieldFormatterNames.CHARLENGTH => $"LENGTH({formatedFieldName})",
                SixnetFieldFormatterNames.COUNT => $"COUNT({formatedFieldName})",
                SixnetFieldFormatterNames.SUM => $"SUM({formatedFieldName})",
                SixnetFieldFormatterNames.MAX => $"MAX({formatedFieldName})",
                SixnetFieldFormatterNames.MIN => $"MIN({formatedFieldName})",
                SixnetFieldFormatterNames.AVG => $"AVG({formatedFieldName})",
                SixnetFieldFormatterNames.JSON_VALUE => $"JSON_VALUE({formatedFieldName},{parameterString})",
                SixnetFieldFormatterNames.JSON_OBJECT => $"JSON_QUERY({formatedFieldName},{parameterString})",
                SixnetFieldFormatterNames.AND => $"({formatedFieldName} & {parameterString})",
                SixnetFieldFormatterNames.OR => $"({formatedFieldName} | {parameterString})",
                SixnetFieldFormatterNames.XOR => $"({formatedFieldName} ^ {parameterString})",
                SixnetFieldFormatterNames.NOT => $"(~{formatedFieldName})",
                SixnetFieldFormatterNames.ADD => $"({formatedFieldName}+{parameterString})",
                SixnetFieldFormatterNames.SUBTRACT => $"({formatedFieldName}-{parameterString})",
                SixnetFieldFormatterNames.MULTIPLY => $"({formatedFieldName}*{parameterString})",
                SixnetFieldFormatterNames.DIVIDE => $"({formatedFieldName}/{parameterString})",
                SixnetFieldFormatterNames.MODULO => $"MOD({formatedFieldName},{parameterString})",
                SixnetFieldFormatterNames.STRING_CONCAT => $"CONCAT({formatedFieldName},{parameterString})",
                SixnetFieldFormatterNames.DATE_TIME_DATE => $"TRUNC({formatedFieldName})",
                SixnetFieldFormatterNames.DATE_TIME_YEAR => $"EXTRACT(YEAR FROM {formatedFieldName})",
                SixnetFieldFormatterNames.DATE_TIME_MONTH => $"EXTRACT(MONTH FROM {formatedFieldName})",
                SixnetFieldFormatterNames.DATE_TIME_DAY => $"EXTRACT(DAY FROM {formatedFieldName})",
                SixnetFieldFormatterNames.DATE_TIME_HOUR => $"EXTRACT(HOUR FROM {formatedFieldName})",
                SixnetFieldFormatterNames.DATE_TIME_MINUTE => $"EXTRACT(MINUTE FROM {formatedFieldName})",
                SixnetFieldFormatterNames.DATE_TIME_SECOND => $"EXTRACT(SECOND FROM {formatedFieldName})",
                SixnetFieldFormatterNames.DATE_TIME_ADD_DAY => $"{formatedFieldName} + INTERVAL {parameterString} DAY",
                SixnetFieldFormatterNames.DATE_TIME_ADD_MONTH => $"{formatedFieldName} + INTERVAL {parameterString} MONTH",
                SixnetFieldFormatterNames.DATE_TIME_ADD_YEAR => $"{formatedFieldName} + INTERVAL {parameterString} YEAR",
                SixnetFieldFormatterNames.TO_LOWER => $"LOWER({formatedFieldName})",
                SixnetFieldFormatterNames.TO_UPPER => $"UPPER({formatedFieldName})",
                SixnetFieldFormatterNames.SUB_STRING => Substring(formatedFieldName, formatOption.Parameter),
                SixnetFieldFormatterNames.STRING_REPLACE => ReplaceString(formatedFieldName, formatOption.Parameter),
                SixnetFieldFormatterNames.MATH_ROUND => $"ROUND({formatedFieldName}, {parameterString})",
                SixnetFieldFormatterNames.MATH_ABS => $"ABS({formatedFieldName})",
                SixnetFieldFormatterNames.MATH_CEILING => $"CEIL({formatedFieldName})",
                SixnetFieldFormatterNames.MATH_FLOOR => $"FLOOR({formatedFieldName})",
                SixnetFieldFormatterNames.MATH_TRUNCATE => $"TRUNC({formatedFieldName}, 0)",
                SixnetFieldFormatterNames.MATH_SIGN => $"SIGN({formatedFieldName})",
                SixnetFieldFormatterNames.MATH_POW => $"POWER({formatedFieldName}, {parameterString})",
                SixnetFieldFormatterNames.MATH_SQRT => $"SQRT({formatedFieldName})",
                SixnetFieldFormatterNames.MATH_EXP => $"EXP({formatedFieldName})",
                SixnetFieldFormatterNames.MATH_LOG => $"LOG({formatOption.Parameter},{formatedFieldName})",
                SixnetFieldFormatterNames.MATH_COS => $"COS({formatedFieldName})",
                SixnetFieldFormatterNames.MATH_SIN => $"SIN({formatedFieldName})",
                SixnetFieldFormatterNames.MATH_TAN => $"TAN({formatedFieldName})",
                SixnetFieldFormatterNames.STRING_INDEX_OF => StringIndexOf(formatedFieldName, formatOption.Parameter),
                SixnetFieldFormatterNames.STRING_LAST_INDEX_OF => StringLastIndexOf(formatedFieldName, formatOption.Parameter),
                SixnetFieldFormatterNames.EXISTS => $"EXISTS{formatedFieldName}",
                SixnetFieldFormatterNames.NOT_EXISTS => $"NOT EXISTS{formatedFieldName}",
                _ => throw new SixnetException($"{SixnetDaMengManager.CurrentDatabaseServerType} does not support field formatter: {formatOption.Name}"),
            };

            return formatedFieldName;
        }

        #region Substring
        string Substring(string formatedFieldName, dynamic parameter)
        {
            if (parameter is Tuple<dynamic, dynamic> tupeParameter)
            {
                return $"SUBSTR({formatedFieldName}, {tupeParameter.Item1 + 1}, {tupeParameter.Item2})";
            }
            else
            {
                return $"SUBSTR({formatedFieldName}, {parameter + 1})";
            }
        }
        #endregion

        #region Replace
        string ReplaceString(string formatedFieldName, object parameter)
        {
            if (parameter is Tuple<dynamic, dynamic> tupeTwoParameter)
            {
                return $"REPLACE({formatedFieldName}, '{tupeTwoParameter.Item1}', '{tupeTwoParameter.Item2}')";
            }
            SixnetDirectThrower.ThrowAppException(true, $"Error field formatter: {formatedFieldName}");
            return string.Empty;
        }
        #endregion

        #region String IndexOf / LastIndexOf
        string StringIndexOf(string formatedFieldName, object parameter)
        {
            if (parameter is Tuple<dynamic, dynamic> tupe)
            {
                var charValue = tupe.Item1;
                var startIndex = tupe.Item2;
                return $"(INSTR({formatedFieldName}, '{charValue}', {startIndex + 1}) - 1)";
            }
            else
            {
                return $"(INSTR({formatedFieldName}, '{parameter}') - 1)";
            }
        }

        string StringLastIndexOf(string formatedFieldName, object parameter)
        {
            if (parameter is Tuple<dynamic, dynamic> tupe)
            {
                var charValue = tupe.Item1;
                return $"(LENGTH({formatedFieldName}) - INSTR(REVERSE({formatedFieldName}), REVERSE('{charValue}')))";
            }
            else
            {
                return $"(LENGTH({formatedFieldName}) - INSTR(REVERSE({formatedFieldName}), REVERSE('{parameter}')))";
            }
        }
        #endregion
    }
}
