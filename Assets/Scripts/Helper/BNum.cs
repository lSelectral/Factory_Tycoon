using System;

/// <summary>
/// class BNum
/// written by Wyjosn 11/14/2017
/// "big Number" class designed to better accomodate numbers that exceed easy use in primitive data types
/// As written, this class maintains two properties for a "BNum" (big Number): an integer for the largest power of 10, and a double for containing 8 significant digits
/// Values are stored as a*10^x with a = #.####### (7 decimals) and x = integers
/// Could be adjusted to maintain additional accuracy, but my use case doesn't care about differences in excess of 1/10millionth scale
/// </summary>
public class BNum   
{
    public double value;
    public int tenPow;
    public int aPow;
	public BNum()
	{
        value = 0;
        tenPow = 0;
    }
    public BNum(double aVal, int aPow)
    {
        this.aPow = aPow;
        value = aVal;
        tenPow = aPow;
        this.CheckValue();
    }

    //custom ToString for default display using Scientific Notation
    public override string ToString() 
    {
        this.CheckValue();
        string outputStr = "";
        double outputVal;
        outputVal = (Math.Floor(this.value * 1000)) / 1000; //truncate to 3 decimals for display
        outputStr = "" + outputVal.ToString("###.0") + " e" + aPow;
        return outputStr;
    }

    //overloaded ToString method for representing big numbers in different styles based on parameter
    public string ToString(string style)
    {
        this.CheckValue();
        string outputStr = "";
        double outputVal;
        int powMult;
        //using symbolic representation (1000 = 1 K, 1000 K = 1 M, 1000 M = 1 B, etc)
        if (style == "sym")
        {
            //intentional int-division to get only whole quotient for use in symbolic representation
            powMult = tenPow / 3; 

            //use remainder to determine how many digits before and after decimal
            switch (tenPow%3)
            {
                case 0:
                    outputVal = (Math.Floor(this.value * 1000)) / 1000; //truncate to 4 digits (3 decimals)
                    outputStr = "" + outputVal.ToString("#.000");
                    break;
                case 1:
                    outputVal = (Math.Floor(this.value * 1000)) / 100; //truncate to 4 digis (2 decimals)
                    outputStr = "" + outputVal.ToString("##.00");
                    break;
                case 2:
                    outputVal = (Math.Floor(this.value * 1000)) / 10; //truncate to 4 digits (1 decimal)
                    outputStr = "" + outputVal.ToString("###.0");
                    break;
                default:
                    return outputStr;
            }

            //appending 'suffix' symbol (K, M, B, T, a, b, ... y, z, aa, ab...) based on power multiple (power/3)
            switch (powMult)
            {
                case 0:
                    outputStr = outputStr + "  ";
                    break;
                case 1:
                    outputStr = outputStr + " K";
                    break;
                case 2:
                    outputStr = outputStr + " M";
                    break;
                case 3:
                    outputStr = outputStr + " B";
                    break;
                case 4:
                    outputStr = outputStr + " T";
                    break;
                case 5:
                    outputStr = outputStr + " a";
                    break;
                case 6:
                    outputStr = outputStr + " b";
                    break;
                case 7:
                    outputStr = outputStr + " c";
                    break;
                case 8:
                    outputStr = outputStr + " d";
                    break;
                case 9:
                    outputStr = outputStr + " e";
                    break;
                case 10:
                    outputStr = outputStr + " f";
                    break;
                case 11:
                    outputStr = outputStr + " g";
                    break;
                case 12:
                    outputStr = outputStr + " h";
                    break;
                case 13:
                    outputStr = outputStr + " i";
                    break;
                case 14:
                    outputStr = outputStr + " j";
                    break;
                case 15:
                    outputStr = outputStr + " k";
                    break;
                case 16:
                    outputStr = outputStr + " l";
                    break;
                case 17:
                    outputStr = outputStr + " m";
                    break;
                /*
                 * easily continue sequence for ever larger numbers
                 */
                default:
                    return outputStr;

            }
            return outputStr;
        }
        //bounce back to default ToString for scientific notation
        else if (style == "sci")
        {
            return this.ToString();
        }
        // output as ##.00 e##, "engineer notation"
        else if (style == "eng")
        {
            outputVal = (Math.Floor(this.value * 1000)) / 100; //truncate to 4 digis (2 decimals)
            outputStr = "" + outputVal.ToString("##.00") + " e" + (aPow - 1);
        }
        return outputStr;
    }

    // discards "excessive differences" (more than 7 orders of magnitude) then aligns powers, performs addition, and reformats result 
    public BNum Plus(BNum num2)
    {
        BNum result; //for storing output
        BNum tempNum = new BNum(num2.value,num2.tenPow); //might not be necessary, but I wasn't confident in whether the reference variable would affect the exterior BNum object or create a copy
        double resVal;
        //check orders of magnitude difference between numbers
        int powDiff = this.tenPow - tempNum.tenPow;
        //shortcuts for excessive power differences to avoid potential overflows just returns the larger ignoring the smaller
        if (powDiff >= 7)
            return this;
        if (powDiff <= -7)
            return num2;
        //adjust the second number to have the same power as this one
        while (powDiff > 0)
        {
            tempNum.value /= 10;
            powDiff -= 1;
        }
        while (powDiff < 0)
        {
            tempNum.value *= 10;
            powDiff += 1;
        }
        //add values now that powers match
        resVal = this.value + tempNum.value;
        //create BNum object with resultant value and power
        result = new BNum(resVal, this.tenPow);
        //verify format of resultant value and power 
        result.CheckValue(); //this is implicitly called in the constructor, but I've called it again out of extra caution unnecessarily
        return result;
    }

    //very similar to the Add method, just different arithmetic operation
    public BNum Minus(BNum num2)
    {
        BNum result; //for storing output
        BNum tempNum = new BNum(num2.value, num2.tenPow); //might not be necessary, but I wasn't confident in whether the reference variable would affect the exterior BNum object or create a copy
        double resVal;
        //check orders of magnitude difference between numbers
        int powDiff = this.tenPow - tempNum.tenPow;
        //shortcuts for excessive power differences to avoid potential overflows just returns the larger ignoring the smaller
        if (powDiff >= 7)
            return this;
        if (powDiff <= -7)
            return num2;
        //adjust the second number to have the same power as this one
        while (powDiff > 0)
        {
            tempNum.value /= 10;
            powDiff -= 1;
        }
        while (powDiff < 0)
        {
            tempNum.value *= 10;
            powDiff += 1;
        }
        //subtract values now that powers match
        resVal = this.value - tempNum.value;
        //create BNum object with resultant value and power
        result = new BNum(resVal, this.tenPow);
        //verify format of resultant value and power 
        result.CheckValue(); //this is implicitly called in the constructor, but I've called it again out of extra caution unnecessarily
        return result;
    }

    //surprisingly trival while simultaneously not a common use case I expect to use
        // a*10^x*b*10^y = (a*b)(10^(x+y))
    public BNum Times(BNum num2)
    {
        BNum result;
        double resVal;
        int resPow;
        // 10^x * 10^y = 10^(x+y)
        resPow = this.tenPow + num2.tenPow;
        // a*b
        resVal = this.value * num2.value;
        result = new BNum(resVal, resPow);
        result.CheckValue(); //again, unnecessary since implicit in the constructor
        return result;
    }

    //also fairly trivial, similar to multiplication, and even less likely to ever be used in my implementation
        // a*10^x / b*10^y = (a/b)(10^(x-y))
    public BNum DivBy(BNum num2)
    {
        BNum result;
        double resVal;
        int resPow;
        // 10^x / 10^y = 10^(x-y)
        resPow = this.tenPow - num2.tenPow;
        // a/b
        resVal = this.value / num2.value; //this may cause slightly unexpected values since it uses 'double' division and rounds 'toward even' by default. Didn't feel like testing since it's an unlikely use case
        result = new BNum(resVal, resPow);
        result.CheckValue(); //again, unnecessary sicne implicit in the constructor
        return result;
    }


    public void CheckValue()
    {
        while( this.value >=10) //increase power until 1<=value<10
        {
            this.value /= 10;
            this.tenPow += 1;
        }
        while( this.value < 1) //decrease power until 1<=value<10
        {
            this.value *= 10;
            this.tenPow -= 1;
        }
        this.value = Math.Floor((this.value * 10000000))/10000000; //"Round Down" to the 7th decimal place
    }
    
}
