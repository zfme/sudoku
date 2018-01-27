﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sudoku
{
    class LoQSolver
    {
        private const int ThreadSayisi = 3;
        // çözüm objesi
        int maxChildCount = 10;
        bool cozuldu = false;
        Board cozumBoard = null;
        List<ConcurrentQueue<BoardDFSQ>> queueList;

        public Board Solve(Board board)
        {
            queueList = new List<ConcurrentQueue<BoardDFSQ>>(65);
            var boardlocks = new object[65];
            for (int i = 0; i < 65; i++)
            {
                queueList.Add(new ConcurrentQueue<BoardDFSQ>());
                boardlocks[i] = new object();
            }
            // ilk queue ya board u ekle
            queueList[0].Enqueue(new BoardDFSQ { Board = board, State = State.Empty });

            var options = new ParallelOptions { MaxDegreeOfParallelism = ThreadSayisi };

            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < ThreadSayisi; i++)
            {
                threads.Add(new Thread(delegate ()
                {
                    Coz();
                }));
            }
            foreach (Thread t in threads)
            {
                t.Start();
            }

            //Parallel.For(0, ThreadSayisi, options, (i, loopState) =>
            //{
            //    Coz();
            //});

            Console.WriteLine("bitti");
            return cozumBoard;
        }

        private void Coz()
        {
            Console.WriteLine("Started thread={0}", Thread.CurrentThread.ManagedThreadId);
            while (!cozuldu)
            {
                for (int q = 64; q >= 0; q--)
                {
                    if (cozuldu)
                    {
                        break;
                    }
                    BoardDFSQ boardDfsq;

                    //bool locked = false;
                    //Monitor.TryEnter(boardlocks[q], ref locked);
                    //if (!locked)
                    //{
                    //    //i = 65;
                    //    continue;
                    //}
                    if (queueList[q].Count == 0)
                    {
                        //Monitor.Exit(boardlocks[q]);
                        continue;
                    }

                    // bakılcak eleman olmalı
                    //try
                    //{
                    queueList[q].TryDequeue(out boardDfsq);
                    //Monitor.Exit(boardlocks[q]);
                    //}
                    //catch (InvalidOperationException ex)
                    //{
                    //    continue;
                    //}
                    if (boardDfsq == null)
                    {
                        continue;
                    }
                    if (boardDfsq.State.Equals(State.Empty))
                    {
                        boardDfsq.State = State.Processing;
                    }
                    else if (boardDfsq.State == State.Processing)
                    {
                        break;
                    }
                    if (boardDfsq.Board.IsSolved())
                    {
                        cozuldu = true;
                        cozumBoard = boardDfsq.Board;
                        break;
                    }
                    int childCount = 0;
                    for (var index = 0; index < 9; index++)
                    {
                        for (var indexy = 0; indexy < 9 && childCount < maxChildCount; indexy++)
                        {
                            if (boardDfsq.Board.Table[index, indexy].Value == 0)
                            {
                                for (int k = 0; k < boardDfsq.Board.Table[index, indexy].PossibleValues.Count; k++)
                                {
                                    byte possibleValue = boardDfsq.Board.Table[index, indexy].PossibleValues[k];
                                    boardDfsq.Board.Table[index, indexy].PossibleValues.RemoveAt(k);
                                    k--;
                                    BoardDFSQ child = boardDfsq.Copy();
                                    child.Board.Table[index, indexy].Value = possibleValue;
                                    child.Board.FillPossibleValues();
                                    if (child.Board.IsValidDfsq())
                                    {
                                        child.State = State.Empty;
                                        child.QueueNdx = q + 1;
                                        queueList[q + 1].Enqueue(child);
                                        childCount++;
                                        if (childCount == maxChildCount)
                                        {
                                            break;
                                        }
                                    }

                                }

                            }
                        }
                    }

                    //queueList[q].Dequeue();
                    //Monitor.Exit(boardlocks[q]);



                    //     Console.WriteLine("Queue check peek-{0}:{1} thread={2}, i={3}", q, boardDfsq,
                    //       Thread.CurrentThread.ManagedThreadId, i);
                }
            }
            Console.WriteLine("Ended thread={0}", Thread.CurrentThread.ManagedThreadId);
        }

    }
}
