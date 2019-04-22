using System;
using System.Collections.Generic;

public class SamplesArray
{
    public static void Main()
    {
        string s = "\r\n";

        int[,] gems = new int[ 6, 6 ];
        Random random = new Random();
        for ( int row = 0; row < 6; row++ )
        {
            for ( int column = 0; column < 6; column++ )
            {
                gems[ column, row ] = random.Next( 1, 4 );
                s += gems[ column, row ] + ",\t";
            }
            s += "\r\n\r\n";
        }

        s += "\r\n\r\n";

        List<int>[] connect = new List<int>[ 36 ];
        for ( int i = 0; i < 36; i++ )
        {
            connect[ i ] = new List<int>();
        }

        int[,] flag = new int[ 6, 6 ];
        for ( int row = 0; row < 6; row++ )
        {
            for ( int column = 0; column < 6; column++ )
            {
                flag[ column, row ] = -1;
            }
        }

        // 分组索引
        int connectIndex = 0;
        for ( int row = 0; row < 6; row++ )
        {
            for ( int column = 0; column < 6; column++ )
            {
                // 当前宝石还没有分组标记
                if ( flag[ column, row ] == -1 )
                {
                    flag[ column, row ] = connectIndex++;
                    // 添加当前宝石到分组
                    connect[ flag[ column, row ] ].Add( gems[ column, row ] );
                }

                // 向右检测相连情况
                for ( int checkColumn = column + 1; checkColumn < 6; checkColumn++ )
                {
                    // 相连就继续检测，不相连中断
                    if ( !Match( column, row, checkColumn, row ) )
                    {
                        break;
                    }
                }

                // 向上检测相连情况
                for ( int checkRow = row + 1; checkRow < 6; checkRow++ )
                {
                    if ( !Match( column, row, column, checkRow ) )
                    {
                        break;
                    }
                }
            }
        }

        for ( int row = 0; row < 6; row++ )
        {
            for ( int column = 0; column < 6; column++ )
            {
                s += flag[ column, row ] + ",\t";
            }
            s += "\r\n\r\n";
        }

        for ( int i = 0; i < 36; i++ )
        {
            if ( connect[ i ].Count > 0 )
            {
                s += "分组标记： " + i + "相连数: " + connect[ i ].Count + ", 宝石类型: " + connect[ i ][ 0 ] + "\r\n";
            }
        }
        Console.WriteLine( s );
        Console.ReadKey();


        bool Match( int column, int row, int checkColumn, int checkRow )
        {
            // 当前宝石与被检测宝石类型相同
            if ( gems[ column, row ] == gems[ checkColumn, checkRow ] )
            {
                // 被检测宝石还没有分组标记
                if ( flag[ checkColumn, checkRow ] == -1 )
                {
                    // 将被检测宝石添加的当前宝石的分组
                    connect[ flag[ column, row ] ].Add( gems[ checkColumn, checkRow ] );
                    // 将被检测宝石的分组标记设置为当前宝石的分组标记
                    flag[ checkColumn, checkRow ] = flag[ column, row ];
                }
                // 被检测宝石已经有分组标记，且与当前宝石分组标记不同
                else if ( flag[ checkColumn, checkRow ] != flag[ column, row ] )
                {
                    // 将被检测宝石与当前宝石的分组合并
                    connect[ flag[ column, row ] ].AddRange( connect[ flag[ checkColumn, checkRow ] ] );
                    // 清空被检测宝石的分组
                    connect[ flag[ checkColumn, checkRow ] ].Clear();
                    // 修改被检测宝石所在分组的所有宝石的分组标记为当前宝石分组标记
                    for ( int i = 0; i < 6; i++ )
                    {
                        for ( int j = 0; j < 6; j++ )
                        {
                            if ( flag[ i, j ] == flag[ checkColumn, checkRow ] )
                            {
                                flag[ i, j ] = flag[ column, row ];
                            }
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}