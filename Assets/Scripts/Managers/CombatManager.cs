using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

/// <summary>
/// 
/// Combats can require food especially really high for attackers (%20 of attack power, NOT SURE)
/// Also combat food and gold requirement will change according to distance between attacking countries.
/// 
/// Combats will be simple. There is 2 important thing.
/// Attack Power and Defense Power
/// Every attack type item produced by a country, increase its attack power by its own unique value
/// Sum of attack power of items determine total attack power of a country.
/// Every defense type item produced by a country, increase its defense power by its own unique value. 
/// 
/// At Defense defender country has %10 more base shield ( That value can increase with special contracts and items)
/// If attacker country's attack power is lower than enemy's or equal to its defense war automatically result as a loss for attacker.
/// If attacker's attack power higher than defender's defense than the country which has highest attack+defense power country will win.
/// And winner will take %40 of money and %25 of resources of defeated country.
/// Every country has maximum 3 lives. If a country beaten in war 3 times, it will be conquered by winner country.
/// When conquring a country, Attacker get all resources (Money, resource, food)
/// If a country has lower than (1,2) lives and win a war at attack it will earn +1 life MAXIMUM 3
/// Lives of country can increase with special contracts. (NEED HIGH AMOUNT VALUABLE RESOURCE)
/// 
/// Every country has a cooldown for next attack ( 30 Minute )
/// 
/// </summary>

public class CombatManager : Singleton<CombatManager>
{
    [SerializeField] GameObject warLog;

    #region Events
    public class OnWarStartedEventArgs : EventArgs
    {
        public Map_Part[] attacker;
        public Map_Part[] defender;
        public Map_Part attackedRegion;
    }

    public event EventHandler<OnWarStartedEventArgs> OnWarStartedEvent;

    public class OnWarEndedEventArgs : EventArgs
    {
        public Map_Part[] attacker;
        public Map_Part[] defender;
        public Map_Part attackedRegion;
        public Map_Part[] winner;
    }

    public event EventHandler<OnWarEndedEventArgs> OnWarEndedEvent;
    #endregion

    /// <summary>
    /// When player or AI, attacking to the surrounding countries, this method will be called.
    /// </summary>
    /// <param name="attacker">Attacker country</param>
    /// <param name="defender">Defender country</param>
    /// <param name="attackedRegion">Chosen attack region, selected by attacker country, if defender country loses, region will lost a live, when all lives consumed only this region will be lost
    /// <param name="defenseBonus"/>Defense bonus as percentage for defender<param name="defenseBonus"/>
    /// <returns>Return Winner country</returns>
    public Map_Part[] AttackCountry(Map_Part[] attacker, Map_Part[] defender, Map_Part attackedRegion, float defenseBonus = 10f)
    {
        if (ResourceManager.Instance.Currency >= CalculateRequiredMoneyForTravel(GetTotalAttackPower(attacker)))
        {
            ResourceManager.Instance.Currency -= CalculateRequiredMoneyForTravel(GetTotalAttackPower(attacker));
        }

        OnWarStartedEvent(this, new OnWarStartedEventArgs()
        {
            attackedRegion = attackedRegion,
            defender = defender,
            attacker = attacker
        });
        Map_Part[] winner = { };
        Map_Part[] loser = { };

        // Defender's defense is so high for attacker, Win for defender
        if (GetTotalAttackPower(attacker) <= (GetTotalDefensePower(defender) * 1.1f))
        {
            winner = defender;
            loser = attacker;
        }
        // Attacker country has greater power so winner is attacker
        else if (GetTotalAttackPower(attacker) + GetTotalDefensePower(attacker) * 1f > GetTotalAttackPower(defender) + GetTotalDefensePower(defender) * (1 + (defenseBonus/100)))
        {
            winner = attacker;
            loser = defender;
        }
        // Defender country has greater power so winner is defender
        else if (GetTotalAttackPower(attacker) + (GetTotalDefensePower(attacker) * 1f) <= GetTotalAttackPower(defender) + GetTotalDefensePower(defender) * (1 + (defenseBonus / 100)))
        {
            winner = defender;
            loser = attacker;
        }

        if (loser == defender)
        {
            attackedRegion.CombatLives--;
            if (attackedRegion.CombatLives == 0)
            {
                attackedRegion.CombatLives = 3;
                OnAttackEnd(attacker, defender, 100, 100);
                ConquerArea(attacker, attackedRegion);
            }
            else
                OnAttackEnd(attacker, defender);

            for (int i = 0; i < attacker.Length; i++)
                attacker[i].CombatLives++;
        }
        else
            OnAttackEnd(defender, loser);

        OnWarEndedEvent?.Invoke(this, new OnWarEndedEventArgs()
        {
            attackedRegion = attackedRegion,
            attacker = attacker,
            defender = defender,
            winner = winner
        });
        Debug.Log(string.Format("Winner: <color=red>{0}</color>",winner));
        return winner;
    }

    public long CalculateRequiredFoodForTravel(float attackPower)
    {
        return (long)(14.7f * attackPower);
    }

    public BNum CalculateRequiredMoneyForTravel(BNum attackPower)
    {
        // TODO check values
        return attackPower * 19.4f;
    }

    public void ConquerArea(Map_Part[] attacker, Map_Part attackedRegion)
    {
        Map_Part[] newRegion = null;
        attackedRegion.GetComponent<Image>().color = attacker[0].GetComponent<Image>().color;

        for (int i = 0; i < attacker.Length; i++)
        {
            List<Map_Part> newList = attacker[i].ConnectedMapParts.ToList();
            newList.Add(attackedRegion);
            attacker[i].ConnectedMapParts = newList.ToArray();
            newRegion = newList.ToArray();
        }
        
        // Remove defeated region from game and connected lists
        //attackedRegion.ConnectedMapParts.Where(map => map != attackedRegion).ToArray();

        foreach (var map in attackedRegion.ConnectedMapParts)
        {
            if (map != attackedRegion)
            {
                var tempList = map.ConnectedMapParts.ToList();
                tempList.Remove(attackedRegion);
                map.ConnectedMapParts = tempList.ToArray();
            }
        }
        attackedRegion.ConnectedMapParts = newRegion;
    }

    public BNum GetTotalAttackPower(Map_Part[] map)
    {
        BNum attackPower = new BNum();
        for (int i = 0; i < map.Length; i++)
            attackPower += map[i].AttackPower;
        return attackPower;
    }

    public BNum GetTotalDefensePower(Map_Part[] map)
    {
        BNum defensePower = new BNum();
        for (int i = 0; i < map.Length; i++)
        {
            defensePower += map[i].DefensePower;
        }
        return defensePower;
    }

    public void OnAttackEnd(Map_Part[] winner, Map_Part[] loser, int resourceLossPercentage = 25, int moneyLossPercentage = 40)
    {
        #region Money and Resource Exchange Part
        Dictionary<BaseResources, BNum> lostResourceDict = new Dictionary<BaseResources, BNum>();
        BNum lostMoney = new BNum();

        for (int i = 0; i < loser.Length; i++)
        {
            lostMoney += loser[i].MoneyAmount * (moneyLossPercentage/100);
            loser[i].MoneyAmount *= ((100-moneyLossPercentage * 1f)/100);

            List<BaseResources> keys = new List<BaseResources>(loser[i].ResourceValueDict.Keys);
            List<BNum> values = new List<BNum>(loser[i].ResourceValueDict.Values);

            for (int j = 0; j < keys.Count; j++)
            {
                var key = keys[j];
                var value = values[j];
                if (!lostResourceDict.ContainsKey(key))
                {
                    lostResourceDict.Add(key, (value * (resourceLossPercentage * 1f / 100)));
                }
                else
                    lostResourceDict[key] += (value * (resourceLossPercentage * 1f / 100));

                // +25% all resources for winner, -25% all resources for loser
                loser[i].ResourceValueDict[key] = (loser[i].ResourceValueDict[key] * ((100 - resourceLossPercentage * 1f) / 100));
            }
        }

        for (int i = 0; i < winner.Length; i++)
        {
            // Add +40% Money for winner
            winner[i].MoneyAmount += lostMoney;

            var keys = new List<BaseResources>(ResourceManager.Instance.resourceValueDict.Keys);
            var values = new List<BNum>(ResourceManager.Instance.resourceValueDict.Values);

            // Iterate for all avaialable resources in game , ADD +25% Resource to winer from loser's resources
            for (int j = 0; j < keys.Count; j++)
            {
                var key = keys[j];

                //  if both winner and loser have this resources simply add value
                if (lostResourceDict.ContainsKey(key) && winner[i].ResourceValueDict.ContainsKey(key))
                {
                    winner[i].ResourceValueDict[key] += lostResourceDict[key];
                }
                // if loser have it but winner don't have this resource, then add to winner resource dictionary
                else if (lostResourceDict.ContainsKey(key) && !winner[i].ResourceValueDict.ContainsKey(key))
                {
                    winner[i].ResourceValueDict.Add(key, lostResourceDict[key]);
                }
            }
        }
        #endregion
    }

    public void RandomAutoAttack()
    {
        // Attacker countries don't include player because player should attack at will
        // But AI can attack player if available
        var attackerCountries = MapManager.Instance.allMaps.ToList();
        List<Map_Part> defenderCountries = attackerCountries;
        attackerCountries.Remove(attackerCountries.Where(c => c.IsPlayerOwned).First());



        //Map_Part[] mostPowerfulCountry = null;
        //var highestAttackPower = allCountries.Max(c => CombatManager.Instance.GetTotalAttackPower(c.ConnectedMapParts));
        //mostPowerfulCountry = allCountries.Where(a => CombatManager.Instance.GetTotalAttackPower(a.ConnectedMapParts) == highestAttackPower).First().ConnectedMapParts;

        System.Random random = new System.Random();
        int index = random.Next(attackerCountries.Count - 1);
        defenderCountries.RemoveAt(index);

        int index2 = random.Next(defenderCountries.Count - 1);

        Map_Part[] attackerCountry = attackerCountries[index2].ConnectedMapParts;
        Map_Part[] defenderCountry = defenderCountries[index].ConnectedMapParts;

        AttackCountry(attackerCountry, defenderCountry, defenderCountry[random.Next(defenderCountry.Length - 1)]);
    }

    public void WriteWarLog()
    {

    }
}