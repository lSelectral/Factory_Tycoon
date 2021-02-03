using System;
/// <summary>
/// class BNum
/// written by Wyjosn 11/14/2017
/// "big Number" class designed to better accomodate numbers that exceed easy use in primitive data types
/// As written, this class maintains two properties for a "BNum" (big Number): an integer for the largest power of 10, and a double for containing 8 significant digits
/// Values are stored as a*10^x with a = #.####### (7 decimals) and x = integers
/// Could be adjusted to maintain additional accuracy, but my use case doesn't care about differences in excess of 1/10millionth scale
/// </summary>
[Serializable]
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

    public static BNum Zero
    {
        get { return new BNum(); }
    }

    //static int GetTenPower(double d)
    //{
    //    int scale = 0;
    //    while (d >= 10)
    //    {
    //        d /= 10;
    //        scale++;
    //    }
    //    return scale;
    //}

    /// <summary>
    /// Convert given Bnum parameter, according to the power difference for processing 2 BNum.
    /// </summary>
    /// <param name="number2">BNum to convert acccording to power difference</param>
    /// <param name="powerDifference">Power difference between 2 BNum</param>
    /// <returns>BNum with fixed power</returns>
    static BNum GetConvertedValue(BNum number2, long powerDifference)
    {
        // After 8 digit accuracy doesn't matter.
        if (powerDifference >= 8)
            return new BNum();
        else if (powerDifference <= -8)
            return number2;

        while (powerDifference > 0)
        {
            number2.value /= 10;
            powerDifference--;
        }
        while (powerDifference < 0)
        {
            number2.value *= 10;
            powerDifference++;
        }
        return number2;
    }

    #region Operators
    // BNum + BNum 
    public static BNum operator +(BNum number) => number;
    public static BNum operator +(BNum number1, BNum number2) =>
        new BNum(number1.value 
            + GetConvertedValue(number2, (number1.tenPow - number2.tenPow)).value, number1.tenPow);
    public static BNum operator +(BNum number1, double number2)
    {
        BNum tempValue = new BNum(number2, 0);
        long powerDifference = number1.tenPow - tempValue.tenPow;
        tempValue = GetConvertedValue(tempValue, powerDifference);
        return new BNum(tempValue.value + number1.value, number1.tenPow);
    }

    // BNum - BNum
    public static BNum operator -(BNum number) => new BNum(-number.value, number.tenPow);
    public static BNum operator -(BNum number1, BNum number2) =>
        new BNum(number1.value - GetConvertedValue(number2, (number1.tenPow - number2.tenPow)).value, number1.tenPow);
    public static BNum operator -(BNum number1, double number2)
    {
        BNum tempValue = new BNum(number2, 0);
        long powerDifference = number1.tenPow - tempValue.tenPow;
        tempValue = GetConvertedValue(tempValue, powerDifference);
        return new BNum(number1.value - tempValue.value, number1.tenPow);
    }

    // BNum*BNum = a*10^x * b*10^y = (a*b)*(10^(x+y))
    public static BNum operator *(BNum number1, BNum number2) =>
        new BNum(number1.value * number2.value, number1.tenPow + number2.tenPow);
    public static BNum operator *(BNum number1, double number2) =>
         new BNum(number1.value * number2, number1.aPow);


    // BNum/BNum = a*10^x / b*10^y = (a/b)(10^(x-y))
    public static BNum operator /(BNum number1, BNum number2) =>
        new BNum(number1.value / number2.value, number1.tenPow - number2.tenPow);
    public static BNum operator /(BNum number1, double number2) =>
        new BNum(number1.value / number2, number1.tenPow);
    public static BNum operator /(double number1, BNum number2) =>
        new BNum((number1 / number2.value), -number2.tenPow);

    // BNum > BNum
    public static bool operator >(BNum number1, BNum number2)
    {
        if (number1.value >= 1 && number1.tenPow > number2.tenPow)
            return true;
        else if (number1.tenPow == number2.tenPow && number1.value > number2.value)
            return true;
        else if (number1.tenPow < number2.tenPow && number1.value >= 1 && number2.value < 1)
            return true;
        else
            return false;
    }
    public static bool operator >(BNum number1, double number2)
    {
        long powerDifference = number1.tenPow - new BNum(number2,0).tenPow;
        BNum convertedValue = GetConvertedValue(new BNum(number2, 0), powerDifference);

        if (number1.value >= 1 && powerDifference > 0)
            return true;
        else if (powerDifference == 0 && number1.value > convertedValue.value)
            return true;
        else if (powerDifference < 0 && number1.value >= 1 && convertedValue.value < 1)
            return true;
        else
            return false;
    }

    // BNum < BNum
    public static bool operator <(BNum number1, BNum number2)
    {
        if (number2 > number1 && number1 != number2)
            return true;
        else
            return false;
    }
    public static bool operator <(BNum number1, double number2)
    {
        long powerDifference = number1.tenPow - new BNum(number2,0).tenPow;
        if (!(number1 > number2) && number1 != GetConvertedValue(new BNum(number2, 0), powerDifference))
            return true;
        else
            return false;
    }

    // BNum >= BNum
    public static bool operator >=(BNum number1, BNum number2)
    {
        if (number2 < number1)
            return true;
        else
            return false;
    }
    public static bool operator >=(BNum number1, double number2)
    {
        if (number1 < number2)
            return false;
        else
            return true;
    }

    // BNum <= BNum
    public static bool operator <=(BNum number1, BNum number2)
    {
        if (number1 > number2)
            return false;
        else
            return true;
    }
    public static bool operator <=(BNum number1, double number2)
    {
        if (number1 > number2)
            return false;
        else
            return true;
    }

    #endregion

    #region Class Methods
    //custom ToString for default display using Scientific Notation
    public override string ToString()
    {
        this.CheckValue();
        string outputStr = "";
        double outputVal;
        outputVal = (Math.Floor(this.value * 1000)) / 1000; //truncate to 3 decimals for display
        outputStr = "" + outputVal.ToString("N6") + " e" + tenPow;
        return outputStr;
    }

    private readonly string[] suffix =
        new string[] { "", "K", "M", "G", "T", "P", "E", "AA", "AB", "BA", "BB", "CA", "CB", "CC" };


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
            switch (tenPow % 3)
            {
                case 0:
                    outputVal = (Math.Floor(this.value * 1000)) / 1000; //truncate to 4 digits (3 decimals)
                    outputStr = outputVal.ToString("#.000");
                    break;
                case 1:
                    outputVal = (Math.Floor(this.value * 1000)) / 100; //truncate to 4 digis (2 decimals)
                    outputStr = outputVal.ToString("##.00");
                    break;
                case 2:
                    outputVal = (Math.Floor(this.value * 1000)) / 10; //truncate to 4 digits (1 decimal)
                    outputStr = outputVal.ToString("###.0");
                    break;
                default:
                    return outputStr;
            }

            if (powMult >= suffix.Length) // Array overflow show standard
                return outputStr;
            else
                outputStr += suffix[powMult];

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

    public void CheckValue()
    {
        while (this.value >= 1000) //increase power until 1<=value<10
        {
            this.value /= 1000;
            this.tenPow += 3;
        }
        while (this.value > 0 && this.value < 1) //decrease power until 1<=value<10
        {
            this.value *= 1000;
            this.tenPow -= 3;
        }
        this.value = Math.Floor((this.value * 10000000)) / 10000000; //"Round Down" to the 7th decimal place
    }
    #endregion
}