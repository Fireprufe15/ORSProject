using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ORSSimplexProject
{
    class Program
    {
        static void Main(string[] args)
        {
            string method = "";
            string enteredString = "", nameOfInputFile = "", nameOfOutputFile = "", newConvertedLine = "";
            List<string> textFileLines, convertedLines = new List<string>();
            string[] EnteredValues, breakupLine;
            bool valuesAccepted = false, hasArtificialVars = false, optimalityCheck, hasUnrestrictedVars = false, hasNegVars = false;
            string maxOrMin = "";
            int amountOfVars = 0, amountOfRows = 0, amountOfCollumns = 0, amountVarsAdded = 0, countEquals = 0;
            double[,] initialSimplexTable = new double[1, 1];
            double[,] firstSimplexTable = new double[1, 1];
            int[] CBVCollumns = new int[1];
            List<int> artificialVariableLineIndexes = new List<int>(), artificialVariableCollumnIndexes = new List<int>();
            List<int> slackVarCollumnIndexes = new List<int>(), excessVarCollumnIndexes = new List<int>(), CCollumnIndexes = new List<int>();
            List<double> decisionVariableFinalAnswers = new List<double>();
            double optimalSolutionZValue = 0;

            //Initial program loop where user enters the command to solve a specific LP model.
            while (valuesAccepted == false)
            {
                Console.Clear();
                Console.WriteLine("Welcome to FireSolver for the Simplex Method");
                Console.WriteLine("Please enter the program parameters like this 'solve <input> <output>'");
                Console.WriteLine("Example: solve xmas.txt xmasout.txt");
                enteredString = Console.ReadLine();
                EnteredValues = enteredString.Split(' ');

                if (EnteredValues[0] == "solve")
                {
                    nameOfInputFile = EnteredValues[1];
                    nameOfOutputFile = EnteredValues[2];
                    valuesAccepted = true;
                }
                else if (EnteredValues[0] == "test")
                {
                    string input = "";
                    for (int i = 1; i < EnteredValues.Length; i++)
                    {
                        input += EnteredValues[i] + ' ';
                    }
                    input = input.Trim();
                    Console.WriteLine(SolveEquation(input, "x"));
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("Something went terribly wrong.  Press any key to continue then please try again.");
                    Console.ReadKey();
                }

            }
            bool methodChosen = false;
            while (!methodChosen)
            {
                Console.Clear();
                Console.WriteLine("Which method should we use?\n1. Primal\n2. Dual\n\nPlease note that if you will be including IP restrictions you must use Dual.");
                string input = Console.ReadLine();
                if (input == "1")
                {
                    method = "primal";
                    methodChosen = true;
                }
                else if (input == "2")
                {
                    method = "dual";
                    methodChosen = true;
                }
                else
                {
                    Console.WriteLine("You entered an invalid selection, please try again!");
                    Console.ReadKey();
                }
            }
            textFileLines = ReadFromFile(nameOfInputFile); //Calls up a method to read the input text file and returns a list of the lines from the text file.

            //Checks the last line of the values retrieved from the textfile to see if the LP contains a unrestricted-in-sign variable statement or negative variables.
            //If it does, a bool is made true for later use and it parses the line to find out which variables are unrestricted.
            string ursLine = textFileLines[textFileLines.Count - 2];
            ursLine = ursLine.TrimEnd();
            string negLine = textFileLines[textFileLines.Count - 1];
            negLine = negLine.TrimEnd();
            List<int> unrestrictedVarPositions = new List<int>();
            List<int> negativeVarPositions = new List<int>();
            string[] lastLineSplit = ursLine.Split(' ');
            if (lastLineSplit[0] == "urs")
            {
                hasUnrestrictedVars = true;
                for (int i = 1; i < lastLineSplit.Length; i++)
                {
                    if (int.Parse(lastLineSplit[i]) > 0)
                    {
                        unrestrictedVarPositions.Add(int.Parse(lastLineSplit[i]));
                    }
                    
                }
            }
            string[] negLineSplit = negLine.Split(' ');
            if (negLineSplit[0] == "<=0")
            {
                hasNegVars = true;
                for (int i = 1; i < negLineSplit.Length; i++)
                {
                    if (int.Parse(negLineSplit[i]) > 0)
                    {
                        negativeVarPositions.Add(int.Parse(negLineSplit[i]));
                    }
                    
                }
            }

            //This code parses the first line of the txt file to discover if it is a min or max.
            //It then contstructs the 0 line for the simplex method.
            breakupLine = textFileLines[0].Split(' ');
            maxOrMin = breakupLine[0].ToLower();
            newConvertedLine += "1";
            for (int i = 1; i < breakupLine.Length; i++)
            {
                amountOfVars++;                                                
                CCollumnIndexes.Add(i);
                if (hasNegVars == true && negativeVarPositions.Contains(i))
                {
                    newConvertedLine += " " + (double.Parse(breakupLine[i])).ToString();
                }
                else
                {
                    newConvertedLine += " " + (double.Parse(breakupLine[i]) * -1).ToString();
                }
                if (hasUnrestrictedVars == true && unrestrictedVarPositions.Contains(i))
                {
                    newConvertedLine += " " + (double.Parse(breakupLine[i])).ToString();
                    amountOfVars++;                        
                }
                                                            
            }
            newConvertedLine += " 0";            

            convertedLines.Add(newConvertedLine);
            newConvertedLine = "";

            //For each of the lines, after the first one, uses these values to construct the constraint rows for the simplex method.
            for (int i = 1; i < textFileLines.Count; i++)
            {
                if ((hasUnrestrictedVars == true && i == textFileLines.Count-2) || (hasNegVars && i == textFileLines.Count-1))  //This part is so that it disregards the last line if it is a urs statement.
                {
                    break;
                }
                breakupLine = textFileLines[i].Split(' ');
                newConvertedLine += "0 ";
                for (int j = 0; j < breakupLine.Length; j++) //goes through each value in the line
                {
                    double n;
                    if (double.TryParse(breakupLine[j], out n) == true) //this if statement checks if the value is in fact a number
                    {
                        if (hasNegVars && negativeVarPositions.Contains(j + 1))
                        {
                            double theValue = double.Parse(breakupLine[j]);
                            theValue = -theValue;
                            newConvertedLine += theValue + " ";
                        }
                        else
                        {
                            newConvertedLine += breakupLine[j] + " ";
                        }
                        
                        if (hasUnrestrictedVars == true && unrestrictedVarPositions.Contains(j+1)) //adds the extra opposite value needed when dealing with unrestricted variables
                        {
                            double theValue = double.Parse(breakupLine[j]);
                            theValue = -theValue;
                            newConvertedLine += theValue + " ";
                        }
                        
                    }
                    else //if the value is not a number, the program assumes it is then the part of the line that indicates the sign of the constraint
                    {
                        switch (breakupLine[j])
                        {
                            case ">=":
                                if (method == "primal")
                                {
                                    if (amountVarsAdded > 0)
                                    {
                                        for (int k = 0; k < amountVarsAdded; k++) //Adds zeroes for all the added variables (slacks, excesses and artificials) that don't apply to this row
                                        {
                                            newConvertedLine += "0 ";
                                        }
                                    }
                                    newConvertedLine += "-1 ";                                 //Adds the excess value to the standard form line                     
                                    newConvertedLine += "1 ";                                  //Adds the abstract value to the standard form line
                      
                                    amountOfVars++;                                            //Increases the amountOfVars variable by two, because of adding 2 new variables
                                    slackVarCollumnIndexes.Add(amountOfVars);
                                    excessVarCollumnIndexes.Add(amountOfVars);
                                    amountOfVars++;
                                    amountVarsAdded++;                                         //This value is increased by two to indicate that two new variables not part of the original LP were added.
                                    amountVarsAdded++;
                                    artificialVariableLineIndexes.Add(i - countEquals);                      //Adds the row number and collumn of the added artifical variables to two lists made to contain these values.
                                    artificialVariableCollumnIndexes.Add(amountOfVars);
                                    hasArtificialVars = true;                                  //Indicates that the LP now has artificial values meaning it should be a 2 phase simplex method.    
                                    break;
                                }
                                if (method == "dual")
                                {
                                    if (amountVarsAdded > 0)
                                    {
                                        for (int k = 0; k < amountVarsAdded; k++) //Adds zeroes for all the added variables (slacks, excesses and artificials) that don't apply to this row
                                        {
                                            newConvertedLine += "0 ";
                                        }
                                    }
                                    newConvertedLine += "-1 ";                                 //Adds the excess value to the standard form line  
                                    amountOfVars++;
                                    amountVarsAdded++;
                                    slackVarCollumnIndexes.Add(amountOfVars);
                                    excessVarCollumnIndexes.Add(amountOfVars);
                                    break;                                    
                                }                      
                                break;
                            case "=":
                                string newStringS = "";
                                foreach (var item in breakupLine)
                                {
                                    if (item == "=")
                                    {
                                        newStringS += "<= ";
                                    }
                                    else
                                    {
                                        newStringS += item + " ";
                                    }
                                }
                                newStringS = newStringS.TrimEnd();
                                string newStringE = "";
                                foreach (var item in breakupLine)
                                {
                                    if (item == "=")
                                    {
                                        newStringE += ">= ";
                                    }
                                    else
                                    {
                                        newStringE += item + " ";
                                    }
                                }
                                newStringE = newStringE.TrimEnd();
                                textFileLines.Insert(i+1, newStringS);
                                textFileLines.Insert(i+1, newStringE);                                                             
                                newConvertedLine = "C";
                                countEquals += 1;
                                break;
                            case "<=":
                                if (amountVarsAdded > 0)
                                {
                                    for (int k = 0; k < amountVarsAdded; k++)   //Adds zeroes for all the added variables (slacks, excesses and artificials) that don't apply to this row
                                    {
                                        newConvertedLine += "0 ";
                                    }
                                }
                                newConvertedLine += "1 ";                                  //Adds the slack value to the standard form line
                                amountOfVars++;                                            //Increases the amountOfVars variable by one
                                slackVarCollumnIndexes.Add(amountOfVars);
                                excessVarCollumnIndexes.Add(amountOfRows);
                                amountVarsAdded++;                                         //This value is increased by 1 to indicate that 1 new variable not part of the original LP was added.
                                break;
                        }
                    }
                }
                newConvertedLine = newConvertedLine.TrimEnd();
                if (newConvertedLine[0] != 'C')
                {
                    convertedLines.Add(newConvertedLine); //Adds the created line to a list of standard form LP lines
                }
                newConvertedLine = "";
            }

            for (int i = 0; i < textFileLines.Count; i++)
            {
                if (textFileLines[i].Contains(" = "))
                {
                    textFileLines.RemoveAt(i);
                }
            }
            
            amountOfRows = textFileLines.Count;
            if (hasUnrestrictedVars == true || hasNegVars)
            {
                amountOfRows = amountOfRows - 2; 
            }
            amountOfCollumns = amountOfVars + 2;
            if (hasArtificialVars == false)  //This part of the if constructs a simplex table for the 1 phase method
            {
                double[,] simplexTable = new double[amountOfRows, amountOfCollumns];
                for (int i = 0; i < amountOfRows; i++)
                {
                    breakupLine = convertedLines[i].Split(' ');
                    for (int j = 0; j < amountOfCollumns; j++)
			        {
                        if (j < breakupLine.Length - 1)
                        {
                            simplexTable[i, j] = double.Parse(breakupLine[j]);    
                        }
                        else
                        {
                            simplexTable[i, j] = 0.0;
                        }
                              
			        }
                    simplexTable[i, amountOfCollumns-1] = double.Parse(breakupLine[breakupLine.Length-1]);
                }
                initialSimplexTable = simplexTable;
                if (method == "dual")
                {
                    foreach (var item in excessVarCollumnIndexes)
                    {
                        int excessRow = 0;
                        for (int i = 0; i < initialSimplexTable.GetLength(0); i++)
                        {
                            if (initialSimplexTable[i, item] == -1)
                            {
                                excessRow = i;
                                break;
                            }
                        }
                        for (int i = 0; i < initialSimplexTable.GetLength(1); i++)
                        {
                            initialSimplexTable[excessRow, i] = -(initialSimplexTable[excessRow, i]);
                        }
                    }
                }
                firstSimplexTable = (double[,])simplexTable.Clone();
                for (int i = 1; i < firstSimplexTable.GetLength(1); i++)
                {
                    firstSimplexTable[0, i] = -firstSimplexTable[0, i];
                }
            }
            else if (hasArtificialVars == true) //This part does Phase 1 of the 2 Phase method and also constructs the simplex table required for Phase 2.
            {
                string wLine = "1 ", tempLine;

                for (int i = 0; i < amountOfVars + 1; i++)
                {
                    wLine += "0 ";
                }
                wLine = wLine.TrimEnd();

                int counter = 0;
                double[,] simplexTable = new double[amountOfRows, amountOfCollumns];
                string[] wLineSplit = new string[1];
                double tempDouble;
                wLineSplit = wLine.Split(' ');
                foreach (var item in artificialVariableLineIndexes)
                {
                    tempLine = convertedLines[item];
                    breakupLine = tempLine.Split(' ');                          
                    for (int i = 1; i < artificialVariableCollumnIndexes[counter]; i++)
                    {
                        tempDouble = double.Parse(breakupLine[i]) + double.Parse(wLineSplit[i]);
                        wLineSplit[i] = tempDouble.ToString("F");
                    }
                    counter++;
                    tempDouble = double.Parse(breakupLine[breakupLine.Length-1]) + double.Parse(wLineSplit[amountOfVars+1]);
                    wLineSplit[amountOfVars+1] = tempDouble.ToString("F");
                }

                wLine = "";
                foreach (var item in wLineSplit)
                {
                    wLine += item + " ";    
                }
                wLine = wLine.TrimEnd();

                breakupLine = wLine.Split(' ');
                for (int i = 0; i < amountOfCollumns; i++)
                {
                    simplexTable[0, i] = double.Parse(breakupLine[i]);    
                }

                for (int i = 1; i < amountOfRows; i++)
                {
                    breakupLine = convertedLines[i].Split(' ');
                    for (int j = 0; j < amountOfCollumns; j++)
                    {
                        if (j < breakupLine.Length - 1)
                        {
                            simplexTable[i, j] = double.Parse(breakupLine[j]);
                        }
                        else
                        {
                            simplexTable[i, j] = 0.0;
                        }    
                    }
                    simplexTable[i, amountOfCollumns - 1] = double.Parse(breakupLine[breakupLine.Length - 1]);
                }
                firstSimplexTable = (double[,])simplexTable.Clone();
                breakupLine = convertedLines[0].Split(' ');
                firstSimplexTable[0, 0] = double.Parse(breakupLine[0]);
                for (int j = 1; j < amountOfCollumns; j++)
                {
                    if (j < breakupLine.Length - 1)
                    {
                        firstSimplexTable[0, j] = -double.Parse(breakupLine[j]);
                    }
                    else
                    {
                        firstSimplexTable[0, j] = 0.0;
                    }
                }


                while (Math.Round(simplexTable[0, amountOfVars + 1], 8) > 0)
                {
                    simplexTable = PivotTable(simplexTable, "min");
                }

                //Now should have optimal W'
                //Next step make new Z line

                string zLineOriginal = convertedLines[0];                
                for (int i = 0; i < amountVarsAdded+1; i++)
                {
                    zLineOriginal += "0 ";   
                }
                zLineOriginal = zLineOriginal.TrimEnd();
                string[] zSplitOriginal = zLineOriginal.Split(' ');
                
                List<double[]> basicVariables = new List<double[]>();

                for (int i = 1; i < amountOfCollumns - amountVarsAdded - 1; i++)
                {
                    int zeroCounter = 0, oneCounter = 0, rowOneLocation = 0;
                    for (int j = 0; j < amountOfRows; j++)
                    {
                        if (simplexTable[j, i] == 0)
                        {
                            zeroCounter++;
                        }
                        else if (simplexTable[j, i] == 1)
                        {
                            oneCounter++;
                            rowOneLocation = j;
                        }
                        else
                        {
                            rowOneLocation = j;
                        }
                    }
                    if (zeroCounter == amountOfRows - 1 && oneCounter == 1)
                    {
                        basicVariables.Add(new double[2] {rowOneLocation, simplexTable[rowOneLocation, i] * simplexTable[rowOneLocation, amountOfCollumns - 1]});
                    }
                    else
                    {
                        basicVariables.Add(new double[2] {rowOneLocation,0});
                    }
                }

                //multiply original Z by basic variable rows to get new Z row
                string[] SplittedNewZ = new string[amountOfCollumns];
                for (int i = 0; i < amountOfCollumns; i++)
                {
                    SplittedNewZ[i] = zSplitOriginal[i];    
                }
                for (int i = 0; i < amountOfVars - amountVarsAdded; i++)
                {
                    if (basicVariables[i][1] != 0)
                    {
                        for (int j = 1; j < amountOfCollumns; j++)
                        {
                            tempDouble = double.Parse(SplittedNewZ[j]) + (simplexTable[(int)(basicVariables[i][0]),j] * -double.Parse(zSplitOriginal[i+1]));
                            SplittedNewZ[j] = tempDouble.ToString("F");   
                        }       
                    }       
                }
                string newZLine = "";
                foreach (var item in SplittedNewZ)
                {
                    newZLine += item +" ";       
                }

                for (int i = 0; i < amountOfCollumns; i++)
                {
                    simplexTable[0, i] = double.Parse(SplittedNewZ[i]);
                }
                foreach (var item in artificialVariableCollumnIndexes)
                {
                    for (int i = 0; i < amountOfRows; i++)
                    {
                        simplexTable[i, item] = 0;
                    }    
                }
                initialSimplexTable = simplexTable;

            }

            if (method == "dual")
            {
                initialSimplexTable = DualPhase(initialSimplexTable, maxOrMin);
            }

            optimalityCheck = isSimplexOptimal(initialSimplexTable,maxOrMin);  //Checks if the simplex table is optimal
            while (optimalityCheck == false)    //Loops until optimal table is achieved
            {
                initialSimplexTable = PivotTable(initialSimplexTable, maxOrMin);    //Pivots the table using a method   
                optimalityCheck = isSimplexOptimal(initialSimplexTable, maxOrMin);  //Checks if the pivoted table is optimal
            }

            CBVCollumns = new int[amountOfRows-1];

            optimalSolutionZValue = initialSimplexTable[0,amountOfCollumns-1];  //Finds the optimal Z value
            for (int i = 1; i < amountOfCollumns-amountVarsAdded-1; i++)    //Loops through all the collumns of the simplex table, checks if they are basic variables and adds the basic variable to a list containing decision variable answers       
            {
                int zeroCounter = 0, oneCounter = 0, rowOneLocation = 0;
                for (int j = 0; j < amountOfRows; j++)
                {
                    if (initialSimplexTable[j,i] == 0)
                    {
                        zeroCounter++;
                    }
                    else if (initialSimplexTable[j,i] == 1)
                    {
                        oneCounter++;
                        rowOneLocation = j;
                    }
                }
                if (zeroCounter == amountOfRows-1 && oneCounter == 1)
                {
                    decisionVariableFinalAnswers.Add(initialSimplexTable[rowOneLocation,i] * initialSimplexTable[rowOneLocation,amountOfCollumns-1]);
                }
                else
                {
                    decisionVariableFinalAnswers.Add(0);
                }
            }

            for (int i = 1; i < amountOfCollumns - 1; i++)    //Loops through all the collumns of the simplex table, checks if they are basic variables and adds the basic variable to a list containing decision variable answers       
            {
                int zeroCounter = 0, oneCounter = 0, rowOneLocation = 0;
                for (int j = 0; j < amountOfRows; j++)
                {
                    if (initialSimplexTable[j, i] == 0)
                    {
                        zeroCounter++;
                    }
                    else if (initialSimplexTable[j, i] == 1)
                    {
                        oneCounter++;
                        rowOneLocation = j;
                    }
                }
                if (zeroCounter == amountOfRows - 1 && oneCounter == 1)
                {
                    CBVCollumns[rowOneLocation - 1] = i;
                }
            }

            if (hasUnrestrictedVars == true)  //If it has unrestricted vars, it calculates the real value of that decision variable and removes the now-useless companion collumn.
            {
                foreach (var item in unrestrictedVarPositions)
                {
                    decisionVariableFinalAnswers[item-1] = decisionVariableFinalAnswers[item-1] - decisionVariableFinalAnswers[item];
                    decisionVariableFinalAnswers.RemoveAt(item);
                }      
            }


            double[] CBV = new double[amountOfRows - 1];
            int c = 0;
            foreach (var item in CBVCollumns) //Get CBV
            {
                CBV[c] = firstSimplexTable[0, item]; 
                c++;
            }

            double[,] BInverse = new double[amountOfRows-1, amountOfRows-1];

            for (int i = 0; i < amountOfRows-1; i++)
            {
                for (int j = 0; j < amountOfRows-1; j++)
                {
                    if (excessVarCollumnIndexes.Contains(slackVarCollumnIndexes[i]))
                    {
                        BInverse[i, j] = -initialSimplexTable[i+1, slackVarCollumnIndexes[j]];
                    }
                    else
                    {
                        BInverse[i, j] = initialSimplexTable[i + 1, slackVarCollumnIndexes[j]];
                    }
                    
                }
            }

            double[] CBVBinverse = new double[amountOfRows - 1];
            for (int i = 0; i < amountOfRows-1; i++)
            {
                if (excessVarCollumnIndexes.Contains(slackVarCollumnIndexes[i]))
                {
                    CBVBinverse[i] = -(initialSimplexTable[0, slackVarCollumnIndexes[i]]);
                }
                else
                {
                    CBVBinverse[i] = initialSimplexTable[0, slackVarCollumnIndexes[i]];
                }
                
            }

            string[,] BInverseString = new string[BInverse.GetLength(0),BInverse.GetLength(1)];
            for (int i = 0; i < BInverse.GetLength(0); i++)
			{
			    for (int j = 0; j < BInverse.GetLength(1); j++)
			    {
			        BInverseString[i,j] = BInverse[i,j].ToString();
			    }
			}

            string[] CBVBInverseString = new string[CBVBinverse.Length];
            for (int i = 1; i < amountOfRows; i++)
            {
                CBVBInverseString[i - 1] = CBVBinverse[i - 1].ToString();
            }

            string RHS = "";
            if (maxOrMin == "max")
            {
                RHS = " >= 0";
            }
            else
            {
                RHS = " <= 0";
            }

            //Sensitivity Analysis Range CBV
            string[] CBVString = new string[CBV.Length];
            List<string> CBVRanges = new List<string>();
            for (int i = 0; i < CBV.Length; i++)
            {
                CBVString[i] = CBV[i].ToString();
            }
            foreach (var item in CCollumnIndexes)
            {
                if (CBVCollumns.Contains(item))
                {
                    List<string> CBVAnswers = new List<string>();
                    string[] CBVChanged = (string[])CBVString.Clone();
                    string[] Ai = new string[amountOfRows - 1];
                    CBVChanged[Array.IndexOf(CBVCollumns, item)] = CBVChanged[Array.IndexOf(CBVCollumns, item)] + " +1∆";
                    string[] ChangedCBVBInverse = MultiplyMatricesHorizontal(CBVChanged, BInverseString).ToArray();

                    for (int i = 0; i < ChangedCBVBInverse.Length; i++)
                    {
                        ChangedCBVBInverse[i] = Simplify(ChangedCBVBInverse[i]);
                    }

                    for (int i = 1; i < amountOfRows; i++)
                    {
                        Ai[i - 1] = firstSimplexTable[i, item].ToString();
                    }
                    //CBVAnswers.Add(MultiplyFlatMatrices(Ai, ChangedCBVBInverse) + " -" + firstSimplexTable[0, item].ToString() + " -1∆");
                    foreach (var col in CCollumnIndexes)
                    {
                        if (col != item)
                        {
                            for (int i = 1; i < amountOfRows; i++)
                            {
                                Ai[i - 1] = firstSimplexTable[i, col].ToString();
                            }
                            string answer = MultiplyFlatMatrices(Ai, ChangedCBVBInverse) + " " + (firstSimplexTable[0, col]*-1).ToString();
                            if (Simplify(answer.TrimEnd()) != "0")
                            {
                                CBVAnswers.Add(Simplify(answer.TrimEnd()));
                            }                            
                        }
                    }

                    string[] bChanged = new string[amountOfRows - 1];
                    for (int j = 0; j < amountOfRows-1; j++)
                    {
                        bChanged[j] = firstSimplexTable[j+1, amountOfCollumns - 1].ToString();                        
                    }
                    CBVAnswers.Add(MultiplyFlatMatrices(bChanged,ChangedCBVBInverse));
                    for (int i = 0; i < ChangedCBVBInverse.Length; i++)
                    {
                        if (excessVarCollumnIndexes.Contains(slackVarCollumnIndexes[i]))
                        {
                            string[] line = ChangedCBVBInverse[i].Split(' ');
                            string ans = "";
                            foreach (var number in line)
                            {
                                if (number[0] == '-')
                                {
                                    ans += "+" + number.Substring(1,number.Length-1) + " ";
                                }
                                else if (number[0] == '+')
                                {
                                    ans += "-" + number.Substring(1, number.Length-1) + " ";
                                }
                                else
                                {
                                    ans += "-" + number + " ";
                                }
                            }
                            ans = Simplify(ans.TrimEnd()).TrimEnd();
                            if (ans != "0")
                            {
                                CBVAnswers.Add(ans);
                            }
                            
                        }
                        else
                        {
                            if (Simplify(ChangedCBVBInverse[i].TrimEnd()) != "0")
                            {
                                CBVAnswers.Add(ChangedCBVBInverse[i]);
                            }
                            
                        }
                    }

                    for (int i = 0; i < CBVAnswers.Count; i++)
                    {
                        double t;
                        if (!double.TryParse(CBVAnswers[i], out t))
                        {
                            CBVAnswers[i] = SolveEquation(CBVAnswers[i] + RHS, "∆");
                        }
                        
                    }

                    double lower = Double.NegativeInfinity, higher = Double.PositiveInfinity;
                    foreach (var answer in CBVAnswers)
                    {
                        double value;
                        if (!double.TryParse(answer, out value))
                        {
                            if (double.TryParse(answer.Split(' ')[2], out value))
                            {

                                if (value < 0 && value > lower)
                                {
                                    lower = value;
                                }
                                else if (value > 0 && value < higher)
                                {
                                    higher = value;
                                }
                            }
                        }
                        
                    }
                    string rangeToAdd = "";
                    if (lower != 0)
                    {
                        rangeToAdd += lower.ToString() + " <=";
                    }
                    rangeToAdd += " ∆ ";
                    if (higher != 0)
                    {
                        rangeToAdd += "<= " + higher.ToString();
                    }
                    CBVRanges.Add("C" + item.ToString() + ": " + rangeToAdd);
                }
            }

            //Sensitivity Analysis Range CNBV
            List<double> CNBV = new List<double>();
            List<string> NBVRanges = new List<string>();
            for (int i = 1; i < amountOfCollumns-amountVarsAdded; i++)
            {
                if (!CBV.Contains(firstSimplexTable[0,i]))
	            {
                    CNBV.Add(firstSimplexTable[0, i]);
	            }
                
            }

            foreach (var item in CCollumnIndexes)
            {
                if (!CBVCollumns.Contains(item))
                {
                    string[] Ai = new string[amountOfRows - 1];
                    for (int i = 1; i < amountOfRows; i++)
                    {
                        Ai[i - 1] = firstSimplexTable[i, item].ToString();
                    }
                    string answer = MultiplyFlatMatrices(Ai, CBVBInverseString)+" "+(-1*firstSimplexTable[0,item]).ToString()+" -1∆ ";
                    if (maxOrMin == "max")
                    {
                        answer += ">= 0";
                    }
                    else
                    {
                        answer += "<= 0";
                    }

                    answer = SolveEquation(answer, "∆");
                    answer = answer.Insert(0, "C" + item.ToString()+": ");
                    NBVRanges.Add(answer);
                }
            }

            //Sensitivity Analysis Range Constraint RHS
            List<string> bRanges = new List<string>();            
            
            for (int i = 0; i < amountOfRows-1; i++)
            {
                string[] bChanged = new string[amountOfRows - 1];
                List<string> bAnswers = new List<string>();
                for (int j = 0; j < amountOfRows-1; j++)
                {
                    bChanged[j] = firstSimplexTable[j+1, amountOfCollumns - 1].ToString();
                    if (j == i)
                    {
                        bChanged[j] = bChanged[j] + " +1∆";
                    }
                }
                bAnswers = MultiplyMatricesVertical(bChanged, BInverseString);
                bAnswers.Add(MultiplyFlatMatrices(CBVBInverseString,bChanged));
                for (int j = 0; j < bAnswers.Count; j++)
                {
                    double t;
                    if (!double.TryParse(bAnswers[j], out t))
                    {
                        bAnswers[j] = SolveEquation(bAnswers[j]+RHS, "∆").TrimEnd();
                    }
                }
                double lower = Double.NegativeInfinity, higher = Double.PositiveInfinity;
                foreach (var item in bAnswers)
                {
                    double value;
                    if (double.TryParse(item.Split(' ')[2],out value))
                    {                      

                        if (value < 0 && value > lower)
                        {
                            lower = value;
                        }
                        else if (value > 0 && value < higher)
                        {
                            higher = value;
                        }
                    }
                }
                string rangeToAdd = "";
                if (lower != 0)
                {
                    rangeToAdd += lower.ToString()+" <=";
                }
                rangeToAdd += " ∆ ";
                if (higher != 0)
                {
                    rangeToAdd += "<= "+higher.ToString();
                }
                bRanges.Add("b"+(i+1).ToString()+": "+rangeToAdd);

            }

            //Sensitivity Analysis Range NBV Collumn
            List<List<string>> ARanges = new List<List<string>>();
            for (int i = 0; i < CCollumnIndexes.Count; i++)
            {
                if (!CBVCollumns.Contains(CCollumnIndexes[i]))
                {

                    string[] AOriginal = new string[amountOfRows - 1];
                    for (int j = 1; j < amountOfRows; j++)
                    {
                        AOriginal[j - 1] = firstSimplexTable[j, CCollumnIndexes[i]].ToString();
                    }
                    List<string> ARangeI = new List<string>();
                    for (int j = 0; j < amountOfRows-1; j++)
                    {
                        string[] AChanged = new string[amountOfRows - 1];
                        List<string> AAnswers = new List<string>();                        
                        AChanged = (string[])AOriginal.Clone();
                        AChanged[j] = AChanged[j] + " +1∆";

                        //AAnswers = MultiplyMatricesVertical(AChanged, BInverseString);
                        AAnswers.Add(MultiplyFlatMatrices(CBVBInverseString, AChanged) + " " + -1*firstSimplexTable[0, CCollumnIndexes[i]]);
                        double t;
                        for (int k = 0; k < AAnswers.Count; k++)
                        {
                            if (!double.TryParse(AAnswers[k],out t))
                            {
                                AAnswers[k] = SolveEquation(AAnswers[k]+RHS, "∆").TrimEnd();
                            }
                        }
                        double lower = Double.NegativeInfinity, higher = Double.PositiveInfinity;
                        foreach (var item in AAnswers)
                        {
                            double value;
                            if (double.TryParse(item.Split(' ')[2], out value))
                            {

                                if (value < 0 && value > lower)
                                {
                                    lower = value;
                                }
                                else if (value > 0 && value < higher)
                                {
                                    higher = value;
                                }
                            }
                        }
                        string rangeToAdd = "";
                        if (lower != 0)
                        {
                            rangeToAdd += lower.ToString() + " <=";
                        }
                        rangeToAdd += " ∆ ";
                        if (higher != 0)
                        {
                            rangeToAdd += "<= " + higher.ToString();
                        }
                        ARangeI.Add("A" + (CCollumnIndexes[i]).ToString()+"."+(j+1).ToString() + ": " + rangeToAdd);
                    }
                    ARanges.Add(ARangeI);

                }
            }

            WriteAnswersToFile(textFileLines, optimalSolutionZValue, decisionVariableFinalAnswers, CBVRanges, NBVRanges, bRanges, ARanges, nameOfOutputFile); //Writes the results to the specified file.
            Console.WriteLine("The solution has been found, please check {0}", nameOfOutputFile);
            Console.ReadKey();
        }

        static List<string> ReadFromFile(string textFileName)
        {
            string lineFromFile;
            List<string> outputText = new List<string>();

            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader(textFileName);
                while ((lineFromFile = file.ReadLine()) != null)
                {
                    outputText.Add(lineFromFile);
                }
                file.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't read the file...");
                Console.WriteLine(e.Message);
            }         

            return outputText;
        }

        static double[,] PivotTable(double[,] simplexTable, string minOrMax)
        {
            int enteringCollumn = 0, winningRow = 0;
            List<double> ratioTests = new List<double>();
            double compareValue, divideValue;
            int rowCount = simplexTable.GetLength(0);
            int collumnCount = simplexTable.GetLength(1);
            compareValue = simplexTable[0,1];
            enteringCollumn = 1;
            double[,] outputTable = new double[rowCount,collumnCount];
            for (int i = 1; i < collumnCount-1; i++)
            {
                if (minOrMax == "max")
                {
                    if (simplexTable[0,i] < compareValue)
                    {
                        compareValue = simplexTable[0, i];
                        enteringCollumn = i;
                    }    
                }
                else if (minOrMax == "min")
                {
                    if (simplexTable[0, i] > compareValue)
                    {
                        compareValue = simplexTable[0, i];
                        enteringCollumn = i;
                    }     
                }
            }

            for (int i = 1; i < rowCount; i++)
            {
                ratioTests.Add(simplexTable[i,collumnCount-1] / simplexTable[i,enteringCollumn]);                  
            }
            compareValue = ratioTests.Max();
            for (int i = 0; i < ratioTests.Count; i++)
            {
                if ((ratioTests[i] < compareValue && ratioTests[i] > 0) || (ratioTests[i] < compareValue && ratioTests[i] == 0 && simplexTable[i + 1, enteringCollumn] > 0))
                {
                    compareValue = ratioTests[i];
                    winningRow = i + 1;
                }
                else if ((ratioTests[i] == compareValue && compareValue > 0) || (ratioTests[i] == compareValue && compareValue == 0 && simplexTable[i + 1, enteringCollumn] > 0)) 
                {
                    compareValue = ratioTests[i];
                    winningRow = i + 1;
                }
            }

            for (int i = 0; i < collumnCount; i++)
            {
                outputTable[winningRow, i] = simplexTable[winningRow, i] / simplexTable[winningRow, enteringCollumn];    
            }

            for (int i = 0; i < rowCount; i++)
            {
                if (i != winningRow)
                {
                    if (simplexTable[i,enteringCollumn] > 0)
                    {
                        divideValue = simplexTable[i, enteringCollumn];
                        for (int j = 0; j < collumnCount; j++)
                        {
                            outputTable[i, j] = simplexTable[i, j] - outputTable[winningRow, j] * divideValue;   
   
                        }    
                    }
                    else if (simplexTable[i,enteringCollumn] == 0)
                    {
                        for (int j = 0; j < collumnCount; j++)
                        {
                            outputTable[i, j] = simplexTable[i, j];   
                        }       
                    }
                    else if (simplexTable[i,enteringCollumn] < 0)
                    {
                        divideValue = simplexTable[i, enteringCollumn];
                        for (int j = 0; j < collumnCount; j++)
                        {
                            outputTable[i, j] = simplexTable[i, j] + outputTable[winningRow, j] * -divideValue;

                        }    
                    }
                }   
            }

            return outputTable;
        }

        static bool isSimplexOptimal(double[,] SimplexTableToCheck, string minOrMax)
        {
            bool isOptimal = true;

            switch (minOrMax)
            {
                case "min":
                    for (int i = 1; i < SimplexTableToCheck.GetLength(1)-1; i++)
                    {
                        if (SimplexTableToCheck[0,i] > 0)
                        {
                            isOptimal = false;    
                        }           
                    }
                    break;
                case "max":
                    for (int i = 1; i < SimplexTableToCheck.GetLength(1); i++)
                    {
                        if (SimplexTableToCheck[0, i] < 0)
                        {
                            isOptimal = false;
                        }
                    }
                    break;
            }

            return isOptimal;
        }

        static void WriteAnswersToFile(List<string> originalLP, double optimalZ, List<double> optimalVars, List<string> CBVRanges, List<string> CNBVRanges, List<string> bRanges, List<List<string>> ARanges, string outputFileName)
        {

            string originalLPString = "";
            foreach (var item in originalLP)
            {
                originalLPString += item.ToString()+Environment.NewLine;
            }

            string decisionVarLine = "";
            int i = 0;
            foreach (var item in optimalVars)
            {
                i++;
                decisionVarLine += "X"+i.ToString()+" = " + item.ToString("F") + " ";    
            }

            string sensitivityAnalysis = Environment.NewLine+"Sensitivity Analysis:"+Environment.NewLine+"==============================";

            string CBVRangesString = Environment.NewLine + "Basic Variable Coefficient Optimal Ranges:" + Environment.NewLine + " ";
            foreach (var item in CBVRanges)
            {
                CBVRangesString += item.ToString() + Environment.NewLine + " ";
            }

            string CNBVRangesString = "Non Basic Variable Coefficient Optimal Ranges:"+Environment.NewLine+" ";
            foreach (var item in CNBVRanges)
            {
                CNBVRangesString += item.ToString() + Environment.NewLine+" ";
            }

            string bRangesString = "Constraint RHS Optimal Ranges:" + Environment.NewLine + " ";
            foreach (var item in bRanges)
            {
                bRangesString += item.ToString() + Environment.NewLine + " ";
            }

            string ARangesString = "NBV Constraint Collumn Value Optimal Ranges:" + Environment.NewLine + " ";
            foreach (var item in ARanges)
            {
                foreach (var row in item)
                {
                    ARangesString += row.ToString() + Environment.NewLine + " ";
                }
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputFileName))
            {
                file.WriteLine("LP Report {0}============================== {0}{0}Original LP{0}==============================", Environment.NewLine);
                file.WriteLine(originalLPString + "{0}Optimal Solution{0}==============================",Environment.NewLine);
                file.WriteLine("Optimal Z Value = "+optimalZ.ToString("F"));
                file.WriteLine(decisionVarLine);
                file.WriteLine(sensitivityAnalysis);
                file.WriteLine(CBVRangesString);
                file.WriteLine(CNBVRangesString);
                file.WriteLine(bRangesString);
                file.WriteLine(ARangesString);
            }            
        }

        static string SolveEquation(string equation, string variableName = null)
        {
            string answer = "";
            equation.TrimEnd();
            string[] EquationArray = equation.Split(' ');
            int signIndex = Array.FindIndex(EquationArray, x => x.Contains('='));
            List<string> equationMoved = new List<string>(), equationParsed = new List<string>(), equationAnswer = new List<string>();
            equationMoved.Add(EquationArray[signIndex]);

            for (int i = 0; i < signIndex; i++)
            {
                if (!EquationArray[i].Contains(variableName))
                {
                    equationMoved.Add((-double.Parse(EquationArray[i])).ToString());
                }
                else
                {
                    equationMoved.Insert(0, EquationArray[i]);
                }
            }

            for (int i = signIndex+1; i < EquationArray.Length; i++)
            {
                if (!EquationArray[i].Contains(variableName))
                {
                    equationMoved.Add(EquationArray[i]);
                }
                else
                {
                    equationMoved.Insert(0, (-double.Parse(EquationArray[i].Substring(0,EquationArray[i].Length-1))).ToString()+variableName);
                }
            }

            signIndex = equationMoved.FindIndex(x => x.Contains('='));
            double LHSCoeff = 0, RHSCoeff = 0;
            for (int i = 0; i < signIndex; i++)
            {
                LHSCoeff += double.Parse(equationMoved[i].Substring(0, equationMoved[i].Length - 1));
            }

            
            equationParsed.Add(LHSCoeff.ToString() + variableName);
            
            

            for (int i = signIndex+1; i < equationMoved.Count; i++)
            {
                RHSCoeff += double.Parse(equationMoved[i]);
            }
            equationParsed.Add(equationMoved[signIndex]);
            equationParsed.Add(RHSCoeff.ToString());

            if (equationParsed[0] != variableName)
            {
                equationParsed[2] = (double.Parse(equationParsed[2]) / double.Parse(equationParsed[0].Substring(0, equationParsed[0].Length - 1))).ToString();
                if ((equationParsed[0][0] == '-') && (equationParsed[1] == ">="))
                {
                    equationParsed[1] = "<=";
                }
                else if ((equationParsed[0][0] == '-') && (equationParsed[1] == "<="))
                {
                    equationParsed[1] = ">=";
                }
                equationParsed[0] = variableName;
            }

            answer = "";
            foreach (var item in equationParsed)
            {
                answer += item + ' ';
            }
            answer.Trim();

            return answer;
        }

        static string MultiplyFlatMatrices(string[] Matrix1, string[] Matrix2)
        {
            string answer = "";

            for (int i = 0; i < Matrix1.Length; i++)
            {
                double m1, m2, ans;
                string answerString ="";
                if (double.TryParse(Matrix1[i],out m1) && double.TryParse(Matrix2[i],out m2))
                {
                    ans = m1 * m2;
                    answer += ans.ToString() + ' ';
                }
                else
                {
                    if ((double.TryParse(Matrix1[i],out m1)))
                    {
                        string[] lineWithVar = Matrix2[i].Split(' ');
                        answerString += (m1 * double.Parse(lineWithVar[0])).ToString()+' ';
                        answerString += (m1 * double.Parse(lineWithVar[1].Substring(0, lineWithVar[1].Length - 1))).ToString() + lineWithVar[1][lineWithVar[1].Length - 1]+' ';
                    }
                    else
                    {
                        m2 = double.Parse(Matrix2[i]);
                        string[] lineWithVar = Matrix1[i].Split(' ');
                        answerString += (m2 * double.Parse(lineWithVar[0])).ToString() + ' ';
                        answerString += (m2 * double.Parse(lineWithVar[1].Substring(0, lineWithVar[1].Length - 1))).ToString() + lineWithVar[1][lineWithVar[1].Length - 1] + ' ';
                    }
                    answerString = answerString.Trim();
                    answer += answerString+' ';
                }           
            }
            answer = answer.Trim();
            return answer;
        }

        static List<string> MultiplyMatricesVertical(string[] MatrixVert, string[,] MatrixBig)
        {
            List<string> answer = new List<string>();

            for (int i = 0; i < MatrixVert.Length; i++)
            {
                string answerString = "";
                double m1, m2, ans;
                for (int j = 0; j < MatrixVert.Length; j++)
                {
                    m2 = double.Parse(MatrixBig[i, j]);
                    if (double.TryParse(MatrixVert[j],out m1))
                    {
                        ans = m1 * m2;
                        answerString += ans.ToString() + ' ';
                    }
                    else
                    {
                        string[] lineWithVar = MatrixVert[j].Split(' ');
                        answerString += (m2 * double.Parse(lineWithVar[0])).ToString() + ' ';
                        answerString += (m2 * double.Parse(lineWithVar[1].Substring(0, lineWithVar[1].Length - 1))).ToString() + lineWithVar[1][lineWithVar[1].Length - 1] + ' ';                    
                    }
                }
                answerString = answerString.Trim();
                answer.Add(answerString);

            }

            return answer;
        }

        static List<string> MultiplyMatricesHorizontal(string[] MatrixHor, string[,] MatrixBig)
        {
            List<string> answer = new List<string>();
            for (int i = 0; i < MatrixHor.Length; i++)
            {
                string answerString = "";
                double m1, m2, ans;
                for (int j = 0; j < MatrixHor.Length; j++)
                {
                    m2 = double.Parse(MatrixBig[j, i]);
                    if (double.TryParse(MatrixHor[j], out m1))
                    {
                        ans = m1 * m2;
                        answerString += ans.ToString() + ' ';
                    }
                    else
                    {
                        string[] lineWithVar = MatrixHor[j].Split(' ');
                        answerString += (m2 * double.Parse(lineWithVar[0])).ToString() + ' ';
                        answerString += (m2 * double.Parse(lineWithVar[1].Substring(0, lineWithVar[1].Length - 1))).ToString() + lineWithVar[1][lineWithVar[1].Length - 1] + ' ';
                    }
                }
                answerString = answerString.Trim();
                answer.Add(answerString);

            }
            return answer;
        }

        static string Simplify(string NumberLine)
        {
            string answer = "";
            double numericPart = 0, num;
            string variablePart = "0∆";
            string[] numbers = NumberLine.Split(' ');

            foreach (var item in numbers)
            {
                if (double.TryParse(item, out num))
                {
                    numericPart += num;
                }
                else
                {
                    variablePart = (double.Parse(variablePart.Substring(0, variablePart.Length - 1)) + double.Parse(item.Substring(0, item.Length - 1))).ToString() + variablePart[variablePart.Length-1];
                }
            }
            if (double.Parse(variablePart.Substring(0, variablePart.Length - 1)) == 0)
            {
                variablePart = "";
            }
            answer = (numericPart.ToString() + " " + variablePart).TrimEnd();

            return answer;
        }

        static double[,] DualPhase(double[,] initialTable, string minOrMax)
        {
            int rhsCol = initialTable.GetLength(1);
            int rows = initialTable.GetLength(0);
            int pivotRow = 0;

            bool optimal = DualPhaseOptimalityCheck(initialTable, ref pivotRow);
            while (!optimal)
            {
                int pivotCol = GetPivotColumn(initialTable, pivotRow);
                initialTable = GenericPivot(initialTable, pivotCol, pivotRow);
                optimal = DualPhaseOptimalityCheck(initialTable, ref pivotRow);
            }

            return initialTable;

        }

        static bool DualPhaseOptimalityCheck(double[,] tableToCheck, ref int pivotRow)
        {
            bool optimal = true;
            int rhsCol = tableToCheck.GetLength(1)-1;
            int rows = tableToCheck.GetLength(0);

            for (int i = 1; i < rows; i++)
            {
                if (tableToCheck[i,rhsCol] < 0)
                {
                   optimal = false;
                    if (tableToCheck[i, rhsCol] < tableToCheck[pivotRow, rhsCol])
                    {
                        pivotRow = i;
                    }
                }
            }

            return optimal;
        }

        static int GetPivotColumn(double[,] tableToCheck, int pivotRow)
        {
            int cols = tableToCheck.GetLength(1);
            int pivotCol = 0;
            double compareVal = Math.Abs(tableToCheck[0, 0] / tableToCheck[pivotRow, 0]);
            for (int i = 0; i < cols-1; i++)
            {
                if (tableToCheck[pivotRow, i] < 0 && Math.Abs(tableToCheck[0, i] / tableToCheck[pivotRow, i]) < compareVal)
                {
                    pivotCol = i;
                    compareVal = Math.Abs(tableToCheck[0, i] / tableToCheck[pivotRow, i]);
                }
            }
            return pivotCol;
        }

        static double[,] GenericPivot(double[,] simplexTable, int enteringCollumn, int winningRow)
        {
            double divideValue;

            int collumnCount = simplexTable.GetLength(1);
            int rowCount = simplexTable.GetLength(0);
            double[,] outputTable = new double[rowCount, collumnCount];

            for (int i = 0; i < collumnCount; i++)
            {
                outputTable[winningRow, i] = simplexTable[winningRow, i] / simplexTable[winningRow, enteringCollumn];
            }

            for (int i = 0; i < rowCount; i++)
            {
                if (i != winningRow)
                {
                    if (simplexTable[i, enteringCollumn] > 0)
                    {
                        divideValue = simplexTable[i, enteringCollumn];
                        for (int j = 0; j < collumnCount; j++)
                        {
                            outputTable[i, j] = simplexTable[i, j] - outputTable[winningRow, j] * divideValue;

                        }
                    }
                    else if (simplexTable[i, enteringCollumn] == 0)
                    {
                        for (int j = 0; j < collumnCount; j++)
                        {
                            outputTable[i, j] = simplexTable[i, j];
                        }
                    }
                    else if (simplexTable[i, enteringCollumn] < 0)
                    {
                        divideValue = simplexTable[i, enteringCollumn];
                        for (int j = 0; j < collumnCount; j++)
                        {
                            outputTable[i, j] = simplexTable[i, j] + outputTable[winningRow, j] * -divideValue;

                        }
                    }
                }
            }
            return outputTable;
        }
    }
}
