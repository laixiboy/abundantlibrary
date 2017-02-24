/**
 * @brief 序列化处理，目前支持序列化DataRow，DataTable
 * @attention 创建此序列化的目的在于寻找一种比Json更短，速度更快，并且支持
 *              .Net下DataRow,DataTable的方式
 * @author wolan
 * @modify 
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using WLLibrary.Log;

namespace WLLibrary
{
    public class SerialHandle
    {
        /// <summary>
        /// 函数：GetColumnTypeID
        /// 功能：根据数据列类型获取类型ID
        /// 作者：wl
        /// </summary>
        /// <param name="columnType"></param>
        /// <param name="columnLength">-1:不支持string类型的长度测量</param>
        /// <returns></returns>
        private static byte GetColumnTypeID(Type columnType, ref byte columnLength)
        {
            //1:bit 2:int8 3:int16 4:int32 5:int64 6:uint8 7:uint16 8:uint32 9:uint64 10:varchar 11:MysqlDateTime 12:DateTime
            byte nColumnType = 0;
            columnLength = 0;
            if (columnType == typeof(System.Boolean))
            {
                nColumnType = 1;
                columnLength = 1;
            }
            else if (columnType == typeof(System.Byte))
            {
                nColumnType = 2;
                columnLength = 2;//short
            }
            else if (columnType == typeof(System.Int16))
            {
                nColumnType = 3;
                columnLength = 2;
            }
            else if (columnType == typeof(System.Int32))
            {
                nColumnType = 4;
                columnLength = 4;
            }
            else if (columnType == typeof(System.Int64))
            {
                nColumnType = 5;
                columnLength = 8;
            }
            else if (columnType == typeof(System.SByte))
            {
                nColumnType = 6;
                columnLength = 2;//short
            }
            else if (columnType == typeof(System.UInt16))
            {
                nColumnType = 7;
                columnLength = 2;
            }
            else if (columnType == typeof(System.UInt32))
            {
                nColumnType = 8;
                columnLength = 4;
            }
            else if (columnType == typeof(System.UInt64))
            {
                nColumnType = 9;
                columnLength = 8;
            }
            else if (columnType == typeof(System.String))
            {
                nColumnType = 10;
            }
            else if (columnType == typeof(MySql.Data.Types.MySqlDateTime))
            {
                nColumnType = 11;
                columnLength = 8;
            }
            else if (columnType == typeof(DateTime))
            {
                nColumnType = 12;
                columnLength = 8;
            }
            else if (columnType == typeof(System.Decimal)
                || columnType == typeof(System.Double))
            {
                nColumnType = 13;
                columnLength = 8;
            }
            else
            {

            }
            return nColumnType;
        }

        private static byte[] GetColumnDataBinary(byte columnTypeId, object obj)
        {
            //1:bit 2:int8 3:int16 4:int32 5:int64 6:uint8 7:uint16 8:uint32 9:uint64 10:varchar 11:MysqlDateTime 12:DateTime
            byte[] ret = null;
            switch (columnTypeId)
            {
                case 1:
                    {
                        Boolean data = false;
                        if (obj != null && obj != DBNull.Value)
                        {
                            data = Convert.ToBoolean(obj);
                        }
                        ret = BitConverter.GetBytes(data);
                    }
                    break;
                case 2://byte
                    {
                        Int16 data = 0;
                        if (obj != null && obj != DBNull.Value)
                        {
                            data = Convert.ToInt16(obj);
                        }
                        ret = BitConverter.GetBytes(data);
                    }
                    break;
                case 3:
                    {
                        Int16 data = 0;
                        if (obj != null && obj != DBNull.Value)
                        {
                            data = Convert.ToInt16(obj);
                        }
                        ret = BitConverter.GetBytes(data);
                    }
                    break;
                case 4:
                    {
                        Int32 data = 0;
                        if (obj != null && obj != DBNull.Value)
                        {
                            data = Convert.ToInt32(obj);
                        }
                        ret = BitConverter.GetBytes(data);
                    }
                    break;
                case 5:
                    {
                        Int64 data = 0;
                        if (obj != null && obj != DBNull.Value)
                        {
                            data = Convert.ToInt64(obj);
                        }
                        ret = BitConverter.GetBytes(data);
                    }
                    break;
                case 6://sbyte
                    {
                        Int16 data = 0;
                        if (obj != null && obj != DBNull.Value)
                        {
                            data = Convert.ToInt16(obj);
                        }
                        ret = BitConverter.GetBytes(data);
                    }
                    break;
                case 7:
                    {
                        UInt16 data = 0;
                        if (obj != null && obj != DBNull.Value)
                        {
                            data = Convert.ToUInt16(obj);
                        }
                        ret = BitConverter.GetBytes(data);
                    }
                    break;
                case 8:
                    {
                        UInt32 data = 0;
                        if (obj != null && obj != DBNull.Value)
                        {
                            data = Convert.ToUInt32(obj);
                        }
                        ret = BitConverter.GetBytes(data);
                    }
                    break;
                case 9:
                    {
                        UInt64 data = 0;
                        if (obj != null && obj != DBNull.Value)
                        {
                            data = Convert.ToUInt64(obj);
                        }
                        ret = BitConverter.GetBytes(data);
                    }
                    break;
                case 10:
                    {

                    }
                    break;
                case 11:
                case 12:
                    {
                        long ticks = 0;
                        if (obj != null && obj != DBNull.Value)
                        {
                            DateTime data = Convert.ToDateTime(obj);
                            ticks = data.Ticks;
                        }
                        ret = BitConverter.GetBytes(ticks);
                    }
                    break;
                case 13:
                    {
                        double data = 0.0;
                        if (obj != null && obj != DBNull.Value)
                        {
                            data = Convert.ToDouble(obj);
                        }
                        ret = BitConverter.GetBytes(data);
                    }
                    break;
                default:
                    {
                        //未识别
                    }
                    break;
            }
            return ret;
        }

        /// <summary>
        /// 函数：GetColumnTypeByID
        /// 功能：根据类型ID获取数据列类型
        /// 作者：wl
        /// </summary>
        /// <param name="columnTypeId"></param>
        /// <returns></returns>
        private static Type GetColumnType(byte columnTypeId)
        {
            //1:bit 2:int8 3:int16 4:int32 5:int64 6:uint8 7:uint16 8:uint32 9:uint64 10:varchar 11:MysqlDateTime 12:DateTime
            Type type = null;
            switch (columnTypeId)
            {
                case 1:
                    {
                        type = typeof(System.Boolean);
                    }
                    break;
                case 2://byte
                    {
                        type = typeof(System.Int16);
                    }
                    break;
                case 3:
                    {
                        type = typeof(System.Int16);
                    }
                    break;
                case 4:
                    {
                        type = typeof(System.Int32);
                    }
                    break;
                case 5:
                    {
                        type = typeof(System.Int64);
                    }
                    break;
                case 6://sbyte
                    {
                        type = typeof(System.Int16);
                    }
                    break;
                case 7:
                    {
                        type = typeof(System.UInt16);
                    }
                    break;
                case 8:
                    {
                        type = typeof(System.UInt32);
                    }
                    break;
                case 9:
                    {
                        type = typeof(System.UInt64);
                    }
                    break;
                case 10:
                    {
                        type = typeof(System.String);
                    }
                    break;
                case 11:
                    {
                        type = typeof(MySql.Data.Types.MySqlDateTime);
                    }
                    break;
                case 12:
                    {
                        type = typeof(DateTime);
                    }
                    break;
                case 13:
                    {
                        type = typeof(System.Double);
                    }
                    break;
            }
            return type;
        }

        /// <summary>
        /// 函数：SetColumnDataByType
        /// 功能：根据类型ID设置数值
        /// 作者：wl
        /// </summary>
        /// <param name="columnTypeId"></param>
        /// <param name="dest"></param>
        /// <param name="idxColumn"></param>
        /// <param name="value"></param>
        private static void SetColumnDataByType(int columnTypeId, DataRow dest, int idxColumn, object value)
        {
            //1:bit 2:int8 3:int16 4:int32 5:int64 6:uint8 7:uint16 8:uint32 9:uint64 10:varchar 11:MysqlDateTime 12:DateTime
            switch (columnTypeId)
            {
                case 1:
                    {
                        dest[idxColumn] = Convert.ToBoolean(value);
                    }
                    break;
                case 2://byte
                    {
                        dest[idxColumn] = Convert.ToInt16(value);
                    }
                    break;
                case 3:
                    {
                        dest[idxColumn] = Convert.ToInt16(value);
                    }
                    break;
                case 4:
                    {
                        dest[idxColumn] = Convert.ToInt32(value);
                    }
                    break;
                case 5:
                    {
                        dest[idxColumn] = Convert.ToInt64(value);
                    }
                    break;
                case 6://sbyte
                    {
                        dest[idxColumn] = Convert.ToInt16(value);
                    }
                    break;
                case 7:
                    {
                        dest[idxColumn] = Convert.ToUInt16(value);
                    }
                    break;
                case 8:
                    {
                        dest[idxColumn] = Convert.ToUInt32(value);
                    }
                    break;
                case 9:
                    {
                        dest[idxColumn] = Convert.ToUInt64(value);
                    }
                    break;
                case 10:
                    {
                        dest[idxColumn] = value.ToString();
                    }
                    break;
                case 11:
                    {
                        dest[idxColumn] = new MySql.Data.Types.MySqlDateTime(value.ToString());
                    }
                    break;
                case 12:
                    {
                        dest[idxColumn] = Convert.ToDateTime(value.ToString());
                    }
                    break;
                case 13:
                    {
                        dest[idxColumn] = Convert.ToDouble(value);
                    }
                    break;
            }
        }

        private static void SetColumnDataByTypeBinary(int columnType, DataRow drDest, int idxColumn,
            byte[] src, ref int idxRead)
        {
            //1:bit 2:int8 3:int16 4:int32 5:int64 6:uint8 7:uint16 8:uint32 9:uint64 10:varchar 11:MysqlDateTime 12:DateTime
            switch (columnType)
            {
                case 1:
                    {
                        drDest[idxColumn] = BitConverter.ToBoolean(src, idxRead);
                        idxRead++;
                    }
                    break;
                case 2:
                    {
                        drDest[idxColumn] = BitConverter.ToInt16(src, idxRead);
                        idxRead += 2;
                    }
                    break;
                case 3:
                    {
                        drDest[idxColumn] = BitConverter.ToInt16(src, idxRead);
                        idxRead += 2;
                    }
                    break;
                case 4:
                    {
                        drDest[idxColumn] = BitConverter.ToInt32(src, idxRead);
                        idxRead += 4;
                    }
                    break;
                case 5:
                    {
                        drDest[idxColumn] = BitConverter.ToInt64(src, idxRead);
                        idxRead += 8;
                    }
                    break;
                case 6:
                    {
                        drDest[idxColumn] = BitConverter.ToInt16(src, idxRead);
                        idxRead += 2;
                    }
                    break;
                case 7:
                    {
                        drDest[idxColumn] = BitConverter.ToUInt16(src, idxRead);
                        idxRead += 2;
                    }
                    break;
                case 8:
                    {
                        drDest[idxColumn] = BitConverter.ToUInt32(src, idxRead);
                        idxRead += 4;
                    }
                    break;
                case 9:
                    {
                        drDest[idxColumn] = BitConverter.ToUInt64(src, idxRead);
                        idxRead += 8;
                    }
                    break;
                case 10:
                    {
                        int columnLen = BitConverter.ToInt32(src, idxRead);
                        idxRead += 4;
                        drDest[idxColumn] = Encoding.UTF8.GetString(src, idxRead, columnLen);
                        idxRead += columnLen;
                    }
                    break;
                case 11:
                    {
                        long ticks = BitConverter.ToInt64(src, idxRead);
                        idxRead += 8;
                        drDest[idxColumn] = new MySql.Data.Types.MySqlDateTime(new DateTime(ticks));
                    }
                    break;
                case 12:
                    {
                        long ticks = BitConverter.ToInt64(src, idxRead);
                        idxRead += 8;
                        drDest[idxColumn] = new DateTime(ticks);
                    }
                    break;
                case 13:
                    {
                        drDest[idxColumn] = BitConverter.ToDouble(src, idxRead);
                        idxRead += 8;
                    }
                    break;
            }
        }

        private static byte[] AllocMemoryForDataTable(DataTable table)
        {
            if (table == null || table.Rows.Count < 1)
                return null;

            DataColumn column = null;
            byte columnLen = 0;
            //table name etc.
            int length = 1 + Encoding.UTF8.GetByteCount(table.TableName) + 2 + 4;
            //column name etc.
            for (int i = 0; i < table.Columns.Count; i++)
            {
                column = table.Columns[i];
                length += 1 + 1 + Encoding.UTF8.GetByteCount(column.ColumnName);//data type                    
                if (10 == GetColumnTypeID(column.DataType, ref columnLen))
                {
                    foreach (DataRow row in table.Rows)
                    {
                        length += 4 + Encoding.UTF8.GetByteCount(row[i].ToString());
                    }
                }
                else
                {
                    length += columnLen * table.Rows.Count;
                }
            }
            return new byte[length];
        }

        private static byte[] AllocMemoryForDataRow(DataRow row)
        {
            if (row == null)
                return null;

            DataColumn column = null;
            byte columnLen = 0;
            //table name etc.
            int length = 1 + Encoding.UTF8.GetByteCount(row.Table.TableName) + 2 + 4;
            //column name etc.
            for (int i = 0; i < row.Table.Columns.Count; i++)
            {
                column = row.Table.Columns[i];
                length += 1 + 1 + Encoding.UTF8.GetByteCount(column.ColumnName);//data type                    
                if (10 == GetColumnTypeID(column.DataType, ref columnLen))
                {
                    length += 4 + Encoding.UTF8.GetByteCount(row[i].ToString());
                }
                else
                {
                    length += columnLen * 1;
                }
            }
            return new byte[length];
        }

        public static byte[] Serial(DataTable table)
        {
            byte[] ret = AllocMemoryForDataTable(table);
            if (ret == null)
                return ret;

            byte columnLen = 0;
            DataColumn column = null;
            byte tableLen = 0, columnType = 0;
            Int16 columnNum = 0;
            int rowNum = 0, columnDataLen = 0;
            byte[] columnData = null;
            string strColumnData = string.Empty;
            byte[] columnTypes = null;
            string tableName = string.Empty;

            #region 数据打包

            int idxWrite = 0;
            tableLen = (byte)Encoding.UTF8.GetBytes(table.TableName, 0, table.TableName.Length, ret, idxWrite + 1);
            ret[idxWrite++] = tableLen;
            idxWrite += tableLen;

            //Column Num      
            columnNum = (Int16)table.Columns.Count;
            columnData = BitConverter.GetBytes(columnNum);
            Buffer.BlockCopy(columnData, 0, ret, idxWrite, columnData.Length);
            idxWrite += columnData.Length;

            //Row Num
            rowNum = table.Rows.Count;
            Buffer.BlockCopy(BitConverter.GetBytes(rowNum), 0, ret, idxWrite, 4);
            idxWrite += 4;

            #region 打包列信息

            columnTypes = new byte[columnNum];
            //column
            for (int i = 0; i < columnNum; i++)
            {
                column = table.Columns[i];
                //column type
                columnType = GetColumnTypeID(column.DataType, ref columnLen);
                columnTypes[i] = columnType;
                ret[idxWrite++] = columnType;
                //column name length + column name                
                columnLen = (byte)Encoding.UTF8.GetBytes(column.ColumnName, 0, column.ColumnName.Length, ret, idxWrite + 1);
                ret[idxWrite++] = columnLen;
                idxWrite += columnLen;
            }
            #endregion

            #region 打包数据
            for (int j = 0; j < rowNum; j++)
            {
                for (int i = 0; i < columnNum; i++)
                {
                    if (columnTypes[i] == 10)
                    {
                        strColumnData = table.Rows[j][i].ToString();
                        columnDataLen = Encoding.UTF8.GetBytes(strColumnData, 0, strColumnData.Length, ret, idxWrite + 4);
                        //string data length
                        columnData = BitConverter.GetBytes(columnDataLen);
                        Buffer.BlockCopy(columnData, 0, ret, idxWrite, columnData.Length);

                        idxWrite += 4;//string data length
                        idxWrite += columnDataLen;//data
                    }
                    else
                    {
                        columnData = GetColumnDataBinary(columnTypes[i], table.Rows[j][i]);
                        //if (columnData == null)
                        //{
                        //    StringBuilder logInfo = new StringBuilder(100);
                        //    logInfo.Append("SerialDataBinary column's NULL:").Append(table.TableName)
                        //        .Append(",").Append(table.Columns[i].ColumnName)
                        //        .Append(",RowNum:").Append(j.ToString());

                        //    MsgSwitch.WriteLog(LOGTYPE.ERROR, logInfo);
                        //}
                        Buffer.BlockCopy(columnData, 0, ret, idxWrite, columnData.Length);
                        idxWrite += columnData.Length;
                    }
                }
            }
            #endregion

            #endregion

            return ret;
        }

        public static DataTable DeSerialToDataTable(byte[] src)
        {
            //table name length(1 bytes)
            //table name

            //columns num(2 bytes)
            //row num(4 bytes)

            //each column type(1 bytes)
            //each column name length(1 bytes)
            //each column name           

            //注意：datetime 使用ticks但是只保留秒部分的数据，精确度只到秒
            //each data length(4 bytes only for string,other type don't have this)
            //each data    

            byte tableLen = 0, columnLen = 0, columnType = 0;
            Int16 columnNum = 0;
            int rowNum = 0;
            string tableName = string.Empty, columnName = string.Empty;
            DataTable dtNew = null;
            DataRow drNew = null;
            byte[] columnTypes = null;

            try
            {
                int idxRead = 0;
                tableLen = src[idxRead++];
                tableName = Encoding.UTF8.GetString(src, idxRead, tableLen);
                idxRead += tableLen;

                dtNew = new DataTable();
                dtNew.TableName = tableName;

                columnNum = BitConverter.ToInt16(src, idxRead);
                idxRead += 2;
                rowNum = BitConverter.ToInt32(src, idxRead);
                idxRead += 4;

                #region 列信息拆包
                columnTypes = new byte[columnNum];
                for (int i = 0; i < columnNum; i++)
                {
                    columnType = src[idxRead++];
                    columnTypes[i] = columnType;
                    columnLen = src[idxRead++];
                    columnName = Encoding.UTF8.GetString(src, idxRead, columnLen);
                    idxRead += columnLen;

                    dtNew.Columns.Add(columnName, GetColumnType(columnType));
                }
                #endregion

                #region 数据拆包
                for (int j = 0; j < rowNum; j++)
                {
                    drNew = dtNew.NewRow();
                    for (int i = 0; i < columnNum; i++)
                    {
                        SetColumnDataByTypeBinary(columnTypes[i], drNew, i, src, ref idxRead);
                    }
                    dtNew.Rows.Add(drNew);
                }
                #endregion

                return dtNew;
            }
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, "DeSerialToDataTable:", ex.ToString());
            }
            return null;
        }

        public static byte[] Serial(DataRow row)
        {
            byte[] ret = AllocMemoryForDataRow(row);
            if (ret == null)
                return ret;

            byte columnLen = 0;
            DataColumn column = null;
            byte tableLen = 0, columnType = 0;
            Int16 columnNum = 0;
            int rowNum = 0, columnDataLen = 0;
            byte[] columnData = null;
            string strColumnData = string.Empty;
            byte[] columnTypes = null;
            string tableName = string.Empty;

            #region 数据打包

            int idxWrite = 0;
            tableLen = (byte)Encoding.UTF8.GetBytes(row.Table.TableName, 0, row.Table.TableName.Length, ret, idxWrite + 1);
            ret[idxWrite++] = tableLen;
            idxWrite += tableLen;

            //Column Num      
            columnNum = (Int16)row.Table.Columns.Count;
            columnData = BitConverter.GetBytes(columnNum);
            Buffer.BlockCopy(columnData, 0, ret, idxWrite, columnData.Length);
            idxWrite += columnData.Length;

            //Row Num
            rowNum = 1;
            Buffer.BlockCopy(BitConverter.GetBytes(rowNum), 0, ret, idxWrite, 4);
            idxWrite += 4;

            #region 打包列信息

            columnTypes = new byte[columnNum];
            //column
            for (int i = 0; i < columnNum; i++)
            {
                column = row.Table.Columns[i];
                //column type
                columnType = GetColumnTypeID(column.DataType, ref columnLen);
                columnTypes[i] = columnType;
                ret[idxWrite++] = columnType;
                //column name length + column name                
                columnLen = (byte)Encoding.UTF8.GetBytes(column.ColumnName, 0, column.ColumnName.Length, ret, idxWrite + 1);
                ret[idxWrite++] = columnLen;
                idxWrite += columnLen;
            }
            #endregion

            #region 打包数据
            for (int i = 0; i < columnNum; i++)
            {
                if (columnTypes[i] == 10)
                {
                    strColumnData = row[i].ToString();
                    columnDataLen = Encoding.UTF8.GetBytes(strColumnData, 0, strColumnData.Length, ret, idxWrite + 4);
                    //string data length
                    columnData = BitConverter.GetBytes(columnDataLen);
                    Buffer.BlockCopy(columnData, 0, ret, idxWrite, columnData.Length);

                    idxWrite += 4;//string data length
                    idxWrite += columnDataLen;//data
                }
                else
                {
                    columnData = GetColumnDataBinary(columnTypes[i], row[i]);
                    Buffer.BlockCopy(columnData, 0, ret, idxWrite, columnData.Length);
                    idxWrite += columnData.Length;
                }
            }
            #endregion

            #endregion

            return ret;
        }

        public static DataRow DeSerialToDataRow(byte[] src)
        {
            //table name length(1 bytes)
            //table name

            //columns num(2 bytes)
            //row num(4 bytes)

            //each column type(1 bytes)
            //each column name length(1 bytes)
            //each column name           

            //注意：datetime 使用ticks但是只保留秒部分的数据，精确度只到秒
            //each data length(4 bytes only for string,other type don't have this)
            //each data    

            byte tableLen = 0, columnLen = 0, columnType = 0;
            Int16 columnNum = 0;
            int rowNum = 0;
            string tableName = string.Empty, columnName = string.Empty;
            DataTable dtNew = null;
            DataRow drNew = null;
            byte[] columnTypes = null;

            try
            {
                int idxRead = 0;
                tableLen = src[idxRead++];
                tableName = Encoding.UTF8.GetString(src, idxRead, tableLen);
                idxRead += tableLen;

                dtNew = new DataTable();
                dtNew.TableName = tableName;

                columnNum = BitConverter.ToInt16(src, idxRead);
                idxRead += 2;
                rowNum = BitConverter.ToInt32(src, idxRead);
                idxRead += 4;

                #region 列信息拆包
                columnTypes = new byte[columnNum];
                for (int i = 0; i < columnNum; i++)
                {
                    columnType = src[idxRead++];
                    columnTypes[i] = columnType;
                    columnLen = src[idxRead++];
                    columnName = Encoding.UTF8.GetString(src, idxRead, columnLen);
                    idxRead += columnLen;

                    dtNew.Columns.Add(columnName, GetColumnType(columnType));
                }
                #endregion

                #region 数据拆包
                for (int j = 0; j < rowNum; j++)
                {
                    drNew = dtNew.NewRow();
                    for (int i = 0; i < columnNum; i++)
                    {
                        SetColumnDataByTypeBinary(columnTypes[i], drNew, i, src, ref idxRead);
                    }
                    dtNew.Rows.Add(drNew);
                }
                #endregion

                return dtNew.Rows[0];
            }
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, "DeSerialToDataTable:", ex.ToString());
            }
            return null;
        }
    }
}
