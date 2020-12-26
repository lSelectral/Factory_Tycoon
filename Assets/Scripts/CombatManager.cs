using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// 
/// Combats can require food especially really high for attackers (%20 of attack power, NOT SURE)
/// 
/// Combats will be simple. There is 2 important thing.
/// Attack Power and Defense Power
/// Every attack type item produced by a country, increase its attack power by its own unique value
/// Sum of attack power of items determine total attack power of a country.
/// Every defense type item produced by a country, increase its defense power by its own unique value. 
/// 
/// At Defense defender country has %10 more shield
/// If attacker country's attack power is lower than enemy's or equal to its defense war automatically result as a loss for attacker.
/// If attacker's attack power higher than defender's defense than the country which has highest attack+defense power country will win.
/// And winner will take %40 of money and %25 of resources of defeated country.
/// Every country has maximum 3 lives. If a country beaten in war 3 times, it will be conquered by winner country.
/// When conquring a country, Attacker get all resources (Money, resource, food)
/// If a country has lower than (1,2) lives and win a war at attack it will earn +1 life MAXIMUM 3
/// Lives of country can increase with special contracts. (NEED HIGH AMOUNT VALUABLE RESOURCE) 
/// 
/// Every country has a cooldown for next attack ( 60 Minute )
/// WHEN PLAYER ATTACK A COUNTRY 10 GAME MINUTE WILL PASS ( NOT FINAL DECISION)
/// 
/// </summary>

public class CombatManager : Singleton<CombatManager>
{
    /// <summary>
    /// When player or AI, attacking to the surrounding countries, this method will be called.
    /// When defender loss, only attackedRegion will loss health. But lost resources are percentage of all regions owned by attacker or defender regarding to who lost the war
    /// </summary>
    /// <param name="attacker">Attacker country, All power as one</param>
    /// <param name="defender">Defender country, All power as one</param>
    /// <param name="attackedRegion">Chosen attack region, selected by attacker country, if defender country loses, region will lost a live, when all lives consumed only this region will be lost
    /// <paramref name="defenseBonus"/>Defense bonus as percentage for defender<paramref name="defenseBonus"/>
    /// <returns></returns>
    public Map_Part[] AttackCountry(Map_Part[] attacker, Map_Part[] defender, Map_Part attackedRegion, float defenseBonus = 10f)
    {
        Map_Part[] winner = { };
        Map_Part[] loser = { };

        // Defender's defense is so high for attacker, Win for defender
        if (GetTotalAttackPower(attacker) <= GetTotalDefensePower(defender) * 1.1f)
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

        Debug.Log(string.Format("Winner: <color=red>{0}</color>",winner));
        return winner;
    }

    public void ConquerArea(Map_Part[] attacker, Map_Part attackedRegion)
    {
        attackedRegion.GetComponent<Image>().color = attacker[0].GetComponent<Image>().color;

        for (int i = 0; i < attacker.Length; i++)
        {
            List<Map_Part> newList = attacker[i].ConnectedMapParts.ToList();
            newList.Add(attackedRegion);
            attacker[i].ConnectedMapParts = newList.ToArray();
        }
        
        // Remove defeated region from game and connected lists
        attackedRegion.ConnectedMapParts = attackedRegion.ConnectedMapParts.Where(map => map != attackedRegion).ToArray();
    }

    public long GetTotalAttackPower(Map_Part[] map)
    {
        long attackPower = 0;
        for (int i = 0; i < map.Length; i++)
            attackPower += map[i].AttackPower;
        return attackPower;
    }

    public long GetTotalDefensePower(Map_Part[] map)
    {
        long defensePower = 0;
        for (int i = 0; i < map.Length; i++)
        {
            defensePower += map[i].DefensePower;
        }
        return defensePower;
    }

    public void OnAttackEnd(Map_Part[] winner, Map_Part[] loser, int resourceLossPercentage = 25, int moneyLossPercentage = 40)
    {
        #region Money and Resource Exchange Part
        Dictionary<BaseResources, long> lostResourceDict = new Dictionary<BaseResources, long>();
        double lostMoney = 0;

        for (int i = 0; i < loser.Length; i++)
        {
            lostMoney += loser[i].MoneyAmount * (moneyLossPercentage/100);
            loser[i].MoneyAmount *= ((100-moneyLossPercentage * 1f)/100);

            List<BaseResources> keys = new List<BaseResources>(loser[i].ResourceValueDict.Keys);
            List<long> values = new List<long>(loser[i].ResourceValueDict.Values);

            for (int j = 0; j < keys.Count; j++)
            {
                var key = keys[j];
                var value = values[j];
                if (!lostResourceDict.ContainsKey(key))
                {
                    lostResourceDict.Add(key, (long)(value * (resourceLossPercentage * 1f / 100)));
                }
                else
                    lostResourceDict[key] += (long)(value * (resourceLossPercentage * 1f / 100));

                // +25% all resources for winner, -25% all resources for loser
                loser[i].ResourceValueDict[key] = (long)(loser[i].ResourceValueDict[key] * ((100 - resourceLossPercentage * 1f) / 100));
            }
        }

        for (int i = 0; i < winner.Length; i++)
        {
            // Add +40% Money for winner
            winner[i].MoneyAmount += lostMoney;

            var keys = new List<BaseResources>(ResourceManager.Instance.resourceValueDict.Keys);
            var values = new List<long>(ResourceManager.Instance.resourceValueDict.Values);

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
}
