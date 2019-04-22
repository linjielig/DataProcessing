using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FightTheLandlord
{
    public enum CardType
    {
        c1,             // 单牌.
        c2,             // 对子.
        ch,             // 双王.
        c3,             // 3不带.
        c31,            // 3带1单.
        c32,            // 3带1对.
        c4,             // 炸弹.
        c412,           // 4带两个单.
        c411,           // 4带一对.
        c123,           // 单顺.
        c1122,          // 双顺.
        c111222,        // 飞机不带.
        c11122234,      // 飞机带单.
        c1112223344,    // 飞机带对.
        c0,             // 错误牌型.
    }
    public class CardTypeData
    {
        public CardType type;
        public int power;
    }

    public class AI : Singleton<AI>
    {
        public void ResetOffset()
        {
            resultsOffset = 0;
            bombResultsOffset = 0;
        }
        public List<GameObject> Get(List<CardData> cardDatas)
        {
            if (cardDatas == null)
            {
                return null;
            }
            cardDatas.Sort();
            AddListLog("搜索数据", cardDatas);

            List<CardData> datas = null;
            CardType type = GetType(cardDatas, out datas);
            Debug.Log("--------------------->>>>>>>>>>>>>>>>>>>>>>> 牌型 " + type);
            switch (type)
            {
                case CardType.c1:
                    return GetSingle(cardDatas, null, false);
                case CardType.c2:
                    return GetPair(cardDatas, null, false);
                case CardType.c3:
                    return GetTriplet(cardDatas, null);
                case CardType.c31:
                    return GetTripletWithSingle(datas, null);
                case CardType.c32:
                    return GetTripletWithPair(datas, null);
                case CardType.c123:
                    return GetSequence(datas, null);
                case CardType.c1122:
                    return GetSequencePair(datas, null);
                case CardType.c111222:
                    return GetSequenceTriplet(datas, null);
                case CardType.c11122234:
                    return GetSequenceTripletWithSingle(datas, null);
                case CardType.c1112223344:
                    return GetSequenceTripletWithPair(datas, null);
                case CardType.c412:
                    return GetQuaternionWithSingle(datas, null);
                case CardType.c411:
                    return GetQuaternionWithSingle(datas, null);
                case CardType.c4:
                    GetBomb(datas, null, false);
                    return GetBomb_();
                case CardType.ch:
                    return null;
                default:
                    return null;
            }

        }

        public SortedDictionary<int, List<GameObject>> cards;
        public List<List<GameObject>> results;
        public List<List<GameObject>> suffixResults;
        public List<List<GameObject>> bombResults;
        int putCount;
        int resultsOffset;
        int bombResultsOffset;

        public AI()
        {
            cards = new SortedDictionary<int, List<GameObject>>();
            results = new List<List<GameObject>>();
            suffixResults = new List<List<GameObject>>();
            bombResults = new List<List<GameObject>>();
            putCount = -1;
            resultsOffset = 0;
            bombResultsOffset = 0;
        }
        public void ConvertListToDictionary(List<GameObject> clones)
        {
            cards.Clear();
            foreach (GameObject clone in clones)
            {
                CardData card = ConvertCloneToData(clone);
                if (!cards.ContainsKey(card.value))
                {
                    cards.Add(card.value, new List<GameObject>());
                }
                cards[card.value].Add(clone);
            }
        }

        // Summary:
        //     是否需要更新搜索结果.
        public bool IsNeedUpdate()
        {
            if (putCount < PlayerUI.putCount)
            {

                putCount = (int)PlayerUI.putCount;
                resultsOffset = 0;
                bombResultsOffset = 0;
                results.Clear();
                bombResults.Clear();
                suffixResults.Clear();
                return true;
            }
            return false;
        }

        CardData ConvertCloneToData(GameObject clone)
        {
            Card card = clone.GetComponent<Card>();
            if (card != null)
            {
                return new CardData(card.suits, card.value);
            }
            else
            {
                Debug.LogError("FightTheLandlord# 获取纸牌数据组件失败.");
                return null;
            }
        }

        List<GameObject> GetTripletWithSingle()
        {
            CardData data = ConvertCloneToData(results[resultsOffset][0]);
            // 搜索除这个三张以外的单牌或可拆出来的单牌.
            GetSingle(null, new List<CardData>() { data }, true);
            // 如果存在单牌,组合三带一返回结果.
            if (suffixResults.Count > 0)
            {
                results[resultsOffset].Add(suffixResults[0][0]);
                return results[resultsOffset++];
            }
            else
            {
                Debug.Log("未搜索到三带单的单牌.");
                GetBomb(null, null, false);
                GetRocket();
                return GetBomb_();
            }
        }

        List<GameObject> GetTripletWithPair()
        {
            CardData data = ConvertCloneToData(results[resultsOffset][0]);
            // 搜索除这个三张以外的单牌或可拆出来的单牌.
            GetPair(null, new List<CardData>() { data }, true);
            // 如果存在单牌,组合三带一返回结果.
            if (suffixResults.Count > 0)
            {
                results[resultsOffset].Add(suffixResults[0][0]);
                results[resultsOffset].Add(suffixResults[0][1]);
                return results[resultsOffset++];
            }
            else
            {
                Debug.Log("未搜索到三带对的对牌.");
                GetBomb(null, null, false);
                GetRocket();
                return GetBomb_();
            }
        }

        List<GameObject> GetSequenceTripletWithSingle()
        {
            List<CardData> excludeDatas = new List<CardData>();
            int tripletCount = 0;
            for (int i = 0; i < results[resultsOffset].Count; i += 3)
            {
                excludeDatas.Add(ConvertCloneToData(results[resultsOffset][i]));
                tripletCount++;
            }

            // 搜索除三张以外的单牌或可拆出来的单牌.
            GetSingle(null, excludeDatas, true);
            // 如果存在对应单牌,组合三顺带单后返回结果.
            if (suffixResults.Count >= tripletCount)
            {
                for (int i = 0; i < suffixResults.Count; i++)
                {
                    results[resultsOffset].Add(suffixResults[i][0]);
                }
                return results[resultsOffset++];
            }
            else
            {
                Debug.Log("未搜索到三顺带单的单牌.");
                GetBomb(null, null, false);
                GetRocket();
                return GetBomb_();
            }
        }

        List<GameObject> GetSequenceTripletWithPair()
        {
            List<CardData> excludeDatas = new List<CardData>();
            int tripletCount = 0;
            for (int i = 0; i < results[resultsOffset].Count; i += 3)
            {
                excludeDatas.Add(ConvertCloneToData(results[resultsOffset][i]));
                tripletCount++;
            }

            // 搜索除三张以外的单牌或可拆出来的对牌.
            GetPair(null, excludeDatas, true);
            // 如果存在对应对牌,组合三顺带对后返回结果.
            if (suffixResults.Count >= tripletCount)
            {
                for (int i = 0; i < suffixResults.Count; i++)
                {
                    results[resultsOffset].Add(suffixResults[i][0]);
                    results[resultsOffset].Add(suffixResults[i][1]);
                }
                return results[resultsOffset++];
            }
            else
            {
                Debug.Log("未搜索到三顺带对的对牌.");
                GetBomb(null, null, false);
                GetRocket();
                return GetBomb_();
            }
        }

        // 有没有符合条件的4带2.
        bool isHave = true;
        List<GameObject> GetQuaternionWithSingle()
        {
            if (isHave)
            {
                List<GameObject> quaternion = GetBomb_();
                if (quaternion == null)
                {
                    Debug.Log("未搜索到4牌型.");
                    isHave = false;
                    GetRocket();
                    return GetBomb_();
                }

                CardData data = ConvertCloneToData(quaternion[0]);
                // 搜索除这个4张以外的单牌或可拆出来的单牌.
                GetSingle(null, new List<CardData>() { data }, true);
                // 如果存在单牌,组合4带2返回结果.
                if (suffixResults.Count > 1)
                {
                    quaternion.Add(suffixResults[0][0]);
                    quaternion.Add(suffixResults[1][0]);
                    return quaternion;
                }
                else
                {
                    Debug.Log("未搜索到4带2的单牌.");
                    isHave = false;
                    GetRocket();
                    return GetBomb_();
                }
            }
            else
            {
                return GetBomb_();
            }
        }

        List<GameObject> GetQuaternionWithPair()
        {
            if (isHave)
            {
                List<GameObject> quaternion = GetBomb_();
                if (quaternion == null)
                {
                    Debug.Log("未搜索到4牌型.");
                    isHave = false;
                    GetRocket();
                    return GetBomb_();
                }

                CardData data = ConvertCloneToData(quaternion[0]);
                // 搜索除这个4张以外的对牌或可拆出来的对牌.
                GetSingle(null, new List<CardData>() { data }, true);
                // 如果存在单牌,组合4带2返回结果.
                if (suffixResults.Count > 0)
                {
                    quaternion.Add(suffixResults[0][0]);
                    quaternion.Add(suffixResults[0][1]);
                    return quaternion;
                }
                else
                {
                    Debug.Log("未搜索到4带对的对牌.");
                    isHave = false;
                    GetRocket();
                    return GetBomb_();
                }
            }
            else
            {
                return GetBomb_();
            }
        }

        // Summary:
        //     获取搜索结果.
        //   备注:
        //     1 如果没有搜索到对应牌型,则返回炸弹.
        //     2 如果搜索到对应牌型则先返回搜索到的牌型,再返回炸弹.
        public List<GameObject> GetResult(CardType type)
        {
            Debug.Log("resultsOffset = " + resultsOffset + ", bombResultsOffset = " + bombResultsOffset);
            // 4带2单.
            if (type == CardType.c412)
            {
                return GetQuaternionWithSingle();
            }
            // 4带一对.
            else if (type == CardType.c411)
            {
                return GetQuaternionWithPair();
            }

            if (results.Count == 0)
            {
                Debug.Log("FightTheLandlord# 未搜索到 " + type + " 的牌");
                GetBomb(null, null, false);
                GetRocket();
                return GetBomb_();
            }
            if (resultsOffset >= results.Count)
            {
                GetBomb(null, null, false);
                GetRocket();
                // 有炸弹则返回炸弹,没有炸弹继续循环返回搜索结果.
                if (bombResults.Count > 0 && bombResultsOffset < bombResults.Count)
                {
                    return GetBomb_();
                }
                else
                {
                    bombResultsOffset = 0;
                    resultsOffset = 0;
                }
            }
            switch (type)
            {
                case CardType.c31:
                    return GetTripletWithSingle();
                case CardType.c32:
                    return GetTripletWithPair();
                case CardType.c11122234:
                    return GetSequenceTripletWithSingle();
                case CardType.c1112223344:
                    return GetSequenceTripletWithPair();
            }
            return results[resultsOffset++];
        }

        public List<GameObject> GetBomb_()
        {
            if (bombResults.Count == 0)
            {
                return null;
            }
            if (bombResultsOffset >= bombResults.Count)
            {
                bombResultsOffset = 0;
                // 炸弹结果已循环一次,如果有对应牌型的搜索结果,则再一次返回搜索到的牌型.
                resultsOffset = 0;
            }
            return bombResults[bombResultsOffset++];
        }

        SortedDictionary<int, List<GameObject>> FilterDatas(List<CardData> excludeDatas)
        {
            SortedDictionary<int, List<GameObject>> tempCards = new SortedDictionary<int, List<GameObject>>();
            foreach (KeyValuePair<int, List<GameObject>> card in cards)
            {
                if (!IsExclude(excludeDatas, card))
                {
                    List<GameObject> clones = new List<GameObject>();
                    clones.AddRange(card.Value);
                    tempCards.Add(card.Key, clones);
                }
            }
            return tempCards;
        }

        bool IsExclude(List<CardData> excludeDatas, KeyValuePair<int, List<GameObject>> card)
        {
            if (excludeDatas == null)
            {
                return false;
            }
            foreach (CardData data in excludeDatas)
            {
                if (data.value == card.Key)
                {
                    return true;
                }
            }
            return false;
        }

        bool CheckDatas(List<CardData> datas, int count, bool isSuffix, CardType type, out CardData data)
        {
            data = null;
            if ((datas == null || datas.Count < count) && isSuffix)
            {
                data = new CardData(0, 2);
            }
            else if ((datas == null || datas.Count < count) && !isSuffix)
            {
                Debug.LogError("FightTheLandlord# 搜索 " + type + " 传入数据错误.");
                return false;
            }
            else
            {
                data = datas[0];
            }
            return true;
        }

        bool CheckDatas(List<CardData> datas, int count, CardType type)
        {
            if (datas == null || datas.Count < count)
            {
                Debug.LogError("FightTheLandlord# 搜索 " + type + " 传入数据错误.");
                return false;
            }
            return true;
        }

        public List<GameObject> GetSingle(List<CardData> datas, List<CardData> excludeDatas, bool isSuffix)
        {
            if (IsNeedUpdate() || isSuffix)
            {
                AddListLog("搜索单张传入数据", datas);
                CardData data = null;
                if (!CheckDatas(datas, 1, isSuffix, CardType.c1, out data))
                {
                    return null;
                }
                SortedDictionary<int, List<GameObject>> filterCards = FilterDatas(excludeDatas);

                // 搜索单张大于的牌.
                foreach (KeyValuePair<int, List<GameObject>> card in filterCards)
                {
                    if (card.Key > data.value && card.Value.Count == 1)
                    {
                        if (isSuffix)
                        {
                            suffixResults.Add(card.Value);
                        }
                        else
                        {
                            results.Add(card.Value);
                        }
                    }
                }
                // 搜索其他拆成单张大于的牌.
                foreach (KeyValuePair<int, List<GameObject>> card in filterCards)
                {
                    if (card.Key > data.value && card.Value.Count > 1)
                    {
                        card.Value.RemoveRange(1, card.Value.Count - 1);
                        if (isSuffix)
                        {
                            suffixResults.Add(card.Value);
                        }
                        else
                        {
                            results.Add(card.Value);
                        }
                    }
                }
                if (isSuffix)
                {
                    AddListListLog("搜索单张后缀", suffixResults);
                }
                else
                {
                    AddListListLog("搜索单张", results);
                }

            }

            if (isSuffix)
            {
                return null;
            }
            return GetResult(CardType.c1);
        }

        public List<GameObject> GetPair(List<CardData> datas, List<CardData> excludeDatas, bool isSuffix)
        {
            if (IsNeedUpdate() || isSuffix)
            {
                AddListLog("搜索对传入数据", datas);
                CardData data = null;
                if (!CheckDatas(datas, 1, isSuffix, CardType.c2, out data))
                {
                    return null;
                }
                SortedDictionary<int, List<GameObject>> filterCards = FilterDatas(excludeDatas);
                foreach (KeyValuePair<int, List<GameObject>> card in filterCards)
                {
                    if (card.Key > data.value && card.Value.Count == 2)
                    {
                        if (isSuffix)
                        {
                            suffixResults.Add(card.Value);
                        }
                        else
                        {
                            results.Add(card.Value);
                        }
                    }
                }
                foreach (KeyValuePair<int, List<GameObject>> card in filterCards)
                {
                    if (card.Key > data.value && card.Value.Count > 2)
                    {
                        card.Value.RemoveRange(2, card.Value.Count - 2);
                        if (isSuffix)
                        {
                            suffixResults.Add(card.Value);
                        }
                        else
                        {
                            results.Add(card.Value);
                        }
                    }
                }
                if (isSuffix)
                {
                    AddListListLog("搜索对后缀", suffixResults);
                }
                else
                {
                    AddListListLog("搜索对", results);
                }
            }
            if (isSuffix)
            {
                return null;
            }
            return GetResult(CardType.c2);
        }

        void GetTriple_(List<CardData> datas, List<CardData> excludeDatas)
        {
            if (IsNeedUpdate())
            {
                AddListLog("搜索三张传入数据", datas);
                CardData data = null;
                if (!CheckDatas(datas, 1, false, CardType.c1, out data))
                {
                    return;
                }
                SortedDictionary<int, List<GameObject>> filterCards = FilterDatas(excludeDatas);

                foreach (KeyValuePair<int, List<GameObject>> card in filterCards)
                {
                    if (card.Key > data.value && card.Value.Count == 3)
                    {
                        results.Add(card.Value);
                    }
                }
                foreach (KeyValuePair<int, List<GameObject>> card in filterCards)
                {
                    if (card.Key > data.value && card.Value.Count > 3)
                    {
                        card.Value.RemoveRange(3, card.Value.Count - 3);
                        results.Add(card.Value);
                    }
                }
                AddListListLog("搜索三张", results);
            }
        }

        public List<GameObject> GetTriplet(List<CardData> datas, List<CardData> excludeDatas)
        {

            GetTriple_(datas, excludeDatas);
            return GetResult(CardType.c3);
        }

        public List<GameObject> GetTripletWithSingle(List<CardData> datas, List<CardData> excludeDatas)
        {
            GetTriple_(datas, excludeDatas);
            return GetResult(CardType.c31);
        }

        public List<GameObject> GetTripletWithPair(List<CardData> datas, List<CardData> excludeDatas)
        {
            GetTriple_(datas, excludeDatas);
            return GetResult(CardType.c32);
        }

        public List<GameObject> GetSequence(List<CardData> datas, List<CardData> excludeDatas)
        {
            if (IsNeedUpdate())
            {
                AddListLog("搜索顺子传入数据", datas);
                if (!CheckDatas(datas, 2, CardType.c123))
                {
                    return null;
                }
                SortedDictionary<int, List<GameObject>> filterCards = FilterDatas(excludeDatas);

                // 是多少张牌的顺子.
                int count = Math.Abs(datas[1].value - datas[0].value) + 1;
                foreach (KeyValuePair<int, List<GameObject>> card in cards)
                {
                    List<GameObject> temp = new List<GameObject>();
                    if (card.Key > datas[0].value)
                    {
                        SearchSequence(card.Key);
                        // 顺子数量相等.
                        if (sequence.Count == count)
                        {
                            temp.AddRange(sequence);
                            results.Add(temp);
                        }
                        // 顺子数量过大.
                        else if (sequence.Count > count)
                        {
                            sequence.RemoveRange(count, sequence.Count - count);
                            temp.AddRange(sequence);
                            results.Add(temp);
                        }
                        sequence.Clear();
                    }
                }
                AddListListLog("搜索顺子", results);
            }

            return GetResult(CardType.c123);
        }

        public List<GameObject> GetSequencePair(List<CardData> datas, List<CardData> excludeDatas)
        {
            if (IsNeedUpdate())
            {
                AddListLog("搜索双顺传入数据", datas);
                if (!CheckDatas(datas, 2, CardType.c1122))
                {
                    return null;
                }
                SortedDictionary<int, List<GameObject>> filterCards = FilterDatas(excludeDatas);

                // 是多少张牌的顺子.
                int count = (Math.Abs(datas[1].value - datas[0].value) + 1) * 2;
                foreach (KeyValuePair<int, List<GameObject>> card in cards)
                {
                    List<GameObject> temp = new List<GameObject>();
                    if (card.Key > datas[0].value)
                    {
                        SearchSequencePair(card.Key);
                        // 顺子数量相等.
                        if (sequence.Count == count)
                        {
                            temp.AddRange(sequence);
                            results.Add(temp);
                        }
                        // 顺子数量过大.
                        else if (sequence.Count > count)
                        {
                            sequence.RemoveRange(count, sequence.Count - count);
                            temp.AddRange(sequence);
                            results.Add(temp);
                        }
                        sequence.Clear();
                    }
                }
                AddListListLog("搜索双顺", results);
            }

            return GetResult(CardType.c1122);
        }

        void GetSequenceTriplet_(List<CardData> datas, List<CardData> excludeDatas)
        {
            if (IsNeedUpdate())
            {
                AddListLog("搜索三顺牌型传入数据", datas);
                if (!CheckDatas(datas, 2, CardType.c111222))
                {
                    return;
                }
                SortedDictionary<int, List<GameObject>> filterCards = FilterDatas(excludeDatas);

                // 是多少张牌的顺子.
                int count = (Math.Abs(datas[1].value - datas[0].value) + 1) * 3;
                foreach (KeyValuePair<int, List<GameObject>> card in filterCards)
                {
                    List<GameObject> temp = new List<GameObject>();
                    if (card.Key > datas[0].value)
                    {
                        SearchSequenceTriplet(card.Key);
                        // 顺子数量相等.
                        if (sequence.Count == count)
                        {
                            temp.AddRange(sequence);
                            results.Add(temp);
                        }
                        // 顺子数量过大.
                        else if (sequence.Count > count)
                        {
                            sequence.RemoveRange(count, sequence.Count - count);
                            temp.AddRange(sequence);
                            results.Add(temp);
                        }
                        sequence.Clear();
                    }
                }
                AddListListLog("搜索三顺牌型", results);
            }
        }
        public List<GameObject> GetSequenceTriplet(List<CardData> datas, List<CardData> excludeDatas)
        {
            GetSequenceTriplet_(datas, excludeDatas);
            return GetResult(CardType.c111222);
        }

        public List<GameObject> GetSequenceTripletWithSingle(List<CardData> datas, List<CardData> excludeDatas)
        {
            GetSequenceTriplet_(datas, excludeDatas);
            return GetResult(CardType.c11122234);
        }

        public List<GameObject> GetSequenceTripletWithPair(List<CardData> datas, List<CardData> excludeDatas)
        {
            GetSequenceTriplet_(datas, excludeDatas);
            return GetResult(CardType.c1112223344);
        }

        List<GameObject> sequence = null;
        public void SearchSequence(int key)
        {
            // 初次搜索.
            if (sequence == null)
            {
                sequence = new List<GameObject>();
            }
            // 只搜索2以下的牌.
            if (key <= 14)
            {
                // 如果当前的牌和后边的牌相连且后边的牌是2以下的牌,则添加当前的牌到顺子,并继续搜索.
                if (cards.ContainsKey(key) && cards.ContainsKey(key + 1) && key + 1 <= 14)
                {
                    sequence.Add(cards[key][0]);
                    SearchSequence(key + 1);
                }
                else if (cards.ContainsKey(key) && key <= 14)
                {
                    sequence.Add(cards[key][0]);
                }
            }
        }
        public void SearchSequencePair(int key)
        {
            // 初次搜索.
            if (sequence == null)
            {
                sequence = new List<GameObject>();
            }
            // 只搜索2以下的牌.
            if (key <= 14)
            {
                // 如果当前的牌和后边的牌相连且后边的牌是2以下的牌,则添加当前的牌到顺子,并继续搜索.
                if (cards.ContainsKey(key) &&
                    cards[key].Count >= 2 &&
                    cards.ContainsKey(key + 1) &&
                    cards[key + 1].Count >= 2 &&
                    key + 1 <= 14)
                {
                    sequence.Add(cards[key][0]);
                    sequence.Add(cards[key][1]);
                    SearchSequencePair(key + 1);
                }
                else if (cards.ContainsKey(key) && cards[key].Count >= 2 && key <= 14)
                {
                    sequence.Add(cards[key][0]);
                    sequence.Add(cards[key][1]);
                }
            }
        }

        public void SearchSequenceTriplet(int key)
        {
            // 初次搜索.
            if (sequence == null)
            {
                sequence = new List<GameObject>();
            }
            // 只搜索2以下的牌.
            if (key <= 14)
            {
                // 如果当前的牌和后边的牌相连且后边的牌是2以下的牌,则添加当前的牌到顺子,并继续搜索.
                if (cards.ContainsKey(key) &&
                    cards[key].Count >= 3 &&
                    cards.ContainsKey(key + 1) &&
                    cards[key + 1].Count >= 3 &&
                    key + 1 <= 14)
                {
                    sequence.Add(cards[key][0]);
                    sequence.Add(cards[key][1]);
                    sequence.Add(cards[key][2]);
                    SearchSequenceTriplet(key + 1);
                }
                else if (cards.ContainsKey(key) && cards[key].Count >= 3 && key <= 14)
                {
                    sequence.Add(cards[key][0]);
                    sequence.Add(cards[key][1]);
                    sequence.Add(cards[key][2]);
                }
            }
        }

        // Summary:
        //     搜索火箭.
        //   备注:
        //     小王的Key = 16, 大王的Key = 17.
        public void GetRocket()
        {
            List<GameObject> rocket = new List<GameObject>();
            foreach (KeyValuePair<int, List<GameObject>> card in cards)
            {
                if (card.Key == 16 || card.Key == 17)
                {
                    rocket.AddRange(card.Value);
                }
            }
            if (rocket.Count == 2)
            {
                bombResults.Add(rocket);
            }
        }

        // Summary:
        //     搜索炸弹.
        public void GetBomb(List<CardData> datas, List<CardData> excludeDatas, bool isBombType)
        {
            // 如果不是炸弹牌型需要搜索.
            if (IsNeedUpdate() || !isBombType)
            {
                isHave = isBombType;
                if (!isBombType)
                {
                    bombResults.Clear();
                }
                AddListLog("搜索炸弹", datas);
                CardData data = null;
                if (!CheckDatas(datas, 1, !isBombType, CardType.c4, out data))
                {
                    return;
                }
                SortedDictionary<int, List<GameObject>> filterCards = FilterDatas(excludeDatas);
                foreach (KeyValuePair<int, List<GameObject>> card in filterCards)
                {
                    if (card.Key > data.value && card.Value.Count == 4)
                    {
                        bombResults.Add(card.Value);
                    }
                }
                AddListListLog("搜索炸弹", bombResults);
            }
        }

        public List<GameObject> GetQuaternionWithSingle(List<CardData> datas, List<CardData> excludeDatas)
        {
            GetBomb(datas, excludeDatas, true);
            return GetResult(CardType.c412);
        }
        public List<GameObject> GetQuaternionWithPair(List<CardData> datas, List<CardData> excludeDatas)
        {
            GetBomb(datas, excludeDatas, true);
            return GetResult(CardType.c411);
        }

        public int Compare(List<CardData> cardDatas1, List<CardData> cardDatas2)
        {
            List<CardData> datas1 = null;
            CardType type1 = GetType(cardDatas1, out datas1);
            List<CardData> datas2 = null;
            CardType type2 = GetType(cardDatas2, out datas2);
            if (type1 == CardType.c0 && type2 != CardType.c0)
            {
                return 1;
            }
            else if (type1 == CardType.c0 || type2 == CardType.c0)
            {
                Debug.LogError("FightTheLandlord 错误牌型.");
                return -1;
            }
            else if (type1 == type2)
            {
                switch (type1)
                {
                    case CardType.c1:
                    case CardType.c2:
                    case CardType.c3:
                        return cardDatas2[0].CompareTo(cardDatas1[0]);
                    case CardType.c31:
                    case CardType.c32:
                    case CardType.c4:
                    case CardType.c412:
                    case CardType.c411:

                        return datas2[0].CompareTo(datas1[0]);
                    case CardType.c123:
                    case CardType.c1122:
                    case CardType.c111222:
                    case CardType.c11122234:
                    case CardType.c1112223344:
                        if (datas1[0].value - datas1[1].value == datas2[0].value - datas2[1].value)
                        {
                            return datas2[0].CompareTo(datas1[0]);
                        }
                        else
                        {
                            return -1;
                        }
                    default:
                        Debug.LogError("FightTheLandlord 未知牌型, type = " + type1);
                        return -1;
                }
            }
            else if (type1 != CardType.ch && type2 == CardType.c4 || type2 == CardType.ch)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        public CardType GetType(List<CardData> cardDatas, out List<CardData> datas)
        {
            datas = null;
            if (IsSingle(cardDatas))
            {
                return CardType.c1;
            }
            else if (IsPair(cardDatas))
            {
                return CardType.c2;
            }
            else if (IsTriplet(cardDatas))
            {
                return CardType.c3;
            }
            else if (IsTripletWithOneSingle(cardDatas, out datas))
            {
                return CardType.c31;
            }
            else if (IsTripletWithOnePair(cardDatas, out datas))
            {
                return CardType.c32;
            }
            else if (IsSequence(cardDatas, out datas))
            {
                return CardType.c123;
            }
            else if (IsSequenceOfPairs(cardDatas, out datas))
            {
                return CardType.c1122;
            }
            else if (IsSequenceOfTriplet(cardDatas, out datas))
            {
                return CardType.c111222;
            }
            else if (IsSequenceOfTripletWithSingle(cardDatas, out datas))
            {
                return CardType.c11122234;
            }
            else if (IsSequenceOfTripletWithPair(cardDatas, out datas))
            {
                return CardType.c1112223344;
            }
            else if (IsQuaternionWithSingle(cardDatas, out datas))
            {
                return CardType.c412;
            }
            else if (IsQuaternionWithPair(cardDatas, out datas))
            {
                return CardType.c411;
            }
            else if (IsBomb(cardDatas, out datas))
            {
                return CardType.c4;
            }
            else if (IsRocket(cardDatas))
            {
                return CardType.ch;
            }
            return CardType.c0;
        }

        public bool IsSingle(List<CardData> cards)
        {
            AddListLog("判断 " + CardType.c1 + " 牌型传入数据", cards);
            if (cards != null && cards.Count == 1)
            {
                return true;
            }
            return false;
        }
        public bool IsPair(List<CardData> cards)
        {
            AddListLog("判断 " + CardType.c2 + " 牌型传入数据", cards);
            if (cards != null && cards.Count == 2 && cards[0].value == cards[1].value)
            {
                return true;
            }
            return false;
        }
        public bool IsTriplet(List<CardData> cards)
        {
            AddListLog("判断 " + CardType.c3 + " 牌型传入数据", cards);
            if (cards != null && cards.Count == 3)
            {
                if (cards[0].value == cards[1].value && cards[0].value == cards[2].value)
                {
                    return true;
                }
            }
            return false;
        }
        // Summary:
        //     datas的第一个数据为三张的牌的大小.
        //     datas的第二个数据为带的对的牌的大小.
        public bool IsTripletWithOneSingle(List<CardData> cards, out List<CardData> datas)
        {
            AddListLog("判断 " + CardType.c31 + " 牌型传入数据", cards);
            datas = null;
            if (cards != null && cards.Count == 4)
            {
                cards.Sort();
                // 前三张牌一样.
                if (
                    cards[0].value == cards[1].value &&
                    cards[0].value == cards[2].value &&
                    cards[0].value != cards[3].value)
                {
                    datas = new List<CardData>();
                    datas.Add(cards[0]);
                    datas.Add(cards[3]);
                    AddListLog("判断 " + CardType.c31 + " 牌型传出数据", datas);
                    return true;
                }
                // 后三张牌一样.
                else if (
                    cards[1].value == cards[2].value &&
                    cards[1].value == cards[3].value &&
                    cards[1].value != cards[0].value)
                {
                    datas = new List<CardData>();
                    datas.Add(cards[3]);
                    datas.Add(cards[0]);
                    AddListLog("判断 " + CardType.c31 + " 牌型传出数据", datas);
                    return true;
                }
                else
                {
                    Debug.Log(CardType.c31 + "传入数据值不对。");

                }
            }
            else
            {
                Debug.Log(CardType.c31 + "传入数据为空或数目不对。");

            }
            return false;
        }
        // Summary:
        //     datas的第一个数据为三张的牌的大小.
        //     datas的第二个数据为带的对的牌的大小.
        public bool IsTripletWithOnePair(List<CardData> cards, out List<CardData> datas)
        {
            AddListLog("判断 " + CardType.c32 + " 牌型传出数据", cards);
            datas = null;
            if (cards != null && cards.Count == 5)
            {
                cards.Sort();

                // 前三张牌一样.
                if (
                    cards[0].value == cards[1].value &&
                    cards[0].value == cards[2].value &&
                    cards[3].value == cards[4].value)
                {
                    datas = new List<CardData>();
                    datas.Add(cards[0]);
                    datas.Add(cards[4]);
                    AddListLog("判断 " + CardType.c32 + " 牌型传出数据", datas);
                    return true;
                }
                // 后三张牌一样.
                else if (
                    cards[2].value == cards[3].value &&
                    cards[2].value == cards[4].value &&
                    cards[0].value == cards[1].value)
                {
                    datas = new List<CardData>();
                    datas.Add(cards[4]);
                    datas.Add(cards[0]);
                    AddListLog("判断 " + CardType.c32 + " 牌型传出数据", datas);
                    return true;
                }
                else
                {
                    Debug.Log(CardType.c32 + "传入数据值不对。");

                }

            }
            else
            {
                Debug.Log(CardType.c32 + "传入数据为空或数目不对。");

            }
            return false;
        }

        // Summary:
        //     datas
        //       第一个数据为顺子开始的牌,第二个数据为顺子结束的牌.
        public bool IsSequence(List<CardData> cards, out List<CardData> datas)
        {
            AddListLog("判断 " + CardType.c123 + " 牌型传入数据", cards);
            datas = null;
            if (cards != null && 5 <= cards.Count && cards.Count < 13)
            {
                cards.Sort();
                for (int i = 0; i < cards.Count - 2; i++)
                {
                    int currentValue = cards[i].value;
                    int nextValue = cards[i + 1].value;
                    // A = 14, 大于14的扑克牌为:2,大小王,这些牌不能加入顺子.
                    if (currentValue <= 14 && nextValue <= 14 && nextValue - currentValue == 1)
                    {
                    }
                    else
                    {
                        Debug.Log(CardType.c123 + "传入数据值不对。");
                        return false;
                    }
                }
                datas = new List<CardData>
                {
                    cards[0],
                    cards[cards.Count - 1]
                };
                AddListLog("判断 " + CardType.c123 + " 牌型传出数据", datas);
                return true;
            }
            else
            {
                Debug.Log(CardType.c123 + "传入数据为空或数目不对。");
            }

            return false;
        }

        public bool IsBomb(List<CardData> cards, out List<CardData> datas)
        {
            AddListLog("判断 " + CardType.c4 + " 牌型传入数据", cards);
            datas = null;
            if (cards != null && cards.Count == 4)
            {
                if (cards[0].value == cards[1].value &&
                    cards[0].value == cards[2].value &&
                    cards[0].value == cards[3].value)
                {
                    datas = new List<CardData>()
                    {
                        cards[0]
                    };
                    AddListLog("判断 " + CardType.c4 + " 牌型传出数据", datas);
                    return true;
                }
                else
                {
                    Debug.Log(CardType.c4 + "传入数据值不对。");
                }
            }
            else
            {
                Debug.Log(CardType.c4 + "传入数据为空或数目不对。");
            }
            return false;
        }

        public bool IsRocket(List<CardData> cards)
        {
            if (cards != null && cards.Count == 2)
            {
                if (cards[0].value + cards[1].value == 33)
                {
                    return true;
                }
                else
                {
                    Debug.Log(CardType.ch + "传入数据值不对。");
                }
            }
            else
            {
                Debug.Log(CardType.ch + "传入数据为空或数目不对。");
            }
            return false;
        }

        // Summary:
        //     判断二顺的牌型.
        //     datas
        //       第一个数据是二顺最小的一个牌的数据.
        //       第二个数据是二顺最大的一个排的数据.
        public bool IsSequenceOfPairs(List<CardData> cards, out List<CardData> datas)
        {
            AddListLog("判断 " + CardType.c1122 + " 牌型传入数据", cards);
            datas = null;
            if (cards != null && cards.Count >= 6 && cards.Count % 2 == 0)
            {
                cards.Sort();
                for (int i = 0; i <= cards.Count - 4; i += 2)
                {
                    int currentPairCard1 = cards[i].value;
                    int currentPairCard2 = cards[i + 1].value;
                    int nextPairCard1 = cards[i + 2].value;
                    int nextPairCard2 = cards[i + 3].value;
                    if (currentPairCard1 == currentPairCard2 &&
                        nextPairCard1 == nextPairCard2 &&
                        currentPairCard1 - nextPairCard1 == -1 &&
                        nextPairCard1 <= 14)
                    {

                    }
                    else
                    {
                        Debug.Log(CardType.c111222 + "传入数据为值不对。");
                        return false;
                    }
                }
                datas = new List<CardData>()
                {
                    cards[0],
                    cards[cards.Count -1]
                };
                AddListLog("判断 " + CardType.c1122 + " 牌型传出数据", datas);
                return true;
            }
            else
            {
                Debug.Log(CardType.c1122 + "传入数据为空或数目不对。");
            }
            return false;
        }

        // Summary:
        //     判断三顺的牌型.
        //     datas
        //       第一个数据是飞机最小三张的一个牌的数据.
        //       第二个数据是飞机最大三张的一个排的数据.
        public bool IsSequenceOfTriplet(List<CardData> cards, out List<CardData> datas)
        {
            AddListLog("判断 " + CardType.c111222 + " 牌型传入数据", cards);
            datas = null;
            if (cards != null && cards.Count >= 6 && cards.Count % 3 == 0)
            {
                cards.Sort();
                for (int i = 0; i <= cards.Count - 6; i += 3)
                {
                    if (cards[i].value == cards[i + 1].value &&
                        cards[i].value == cards[i + 2].value &&
                        cards[i + 3].value == cards[i + 4].value &&
                        cards[i + 3].value == cards[i + 5].value &&
                        cards[i + 3].value - cards[i].value == 1 &&
                        cards[i + 5].value <= 14)
                    {

                    }
                    else
                    {
                        Debug.Log(CardType.c111222 + "传入数据值不对。");
                        return false;
                    }
                }
                datas = new List<CardData>()
                {
                    cards[0],
                    cards[cards.Count -1]
                };
                AddListLog("判断 " + CardType.c111222 + " 牌型传出数据", datas);
                return true;
            }
            else
            {
                Debug.Log(CardType.c111222 + "传入数据为空或数目不对。");
            }
            return false;
        }

        // Summary:
        //     判断飞机带单的牌型.
        //     datas
        //       第一个数据是飞机最小三张的一个牌的数据.
        //       第二个数据是飞机最大三张的一个排的数据.
        public bool IsSequenceOfTripletWithSingle(List<CardData> cards, out List<CardData> datas)
        {
            AddListLog("判断 " + CardType.c11122234 + " 牌型传入数据", cards);
            datas = null;
            if (cards != null && cards.Count >= 8 && cards.Count % 4 == 0)
            {
                cards.Sort();
                // 寻找三顺开始的位置.
                int start = -1;
                for (int i = 0; i < cards.Count - 3; i++)
                {
                    if (cards[i].value == cards[i + 1].value &&
                        cards[i].value == cards[i + 2].value)
                    {
                        start = i;
                        break;
                    }
                }
                if (start == -1)
                {
                    Debug.Log(CardType.c11122234 + "未找到三张。");
                    return false;
                }

                // 寻找三顺结束位置.
                int end = -1;
                int tripletCount = 0;
                for (int i = start; i <= cards.Count - 6; i += 3)
                {
                    if (cards[i].value == cards[i + 1].value &&
                        cards[i].value == cards[i + 2].value &&
                        cards[i + 3].value == cards[i + 4].value &&
                        cards[i + 3].value == cards[i + 5].value &&
                        cards[i + 3].value - cards[i].value == 1)
                    {
                        if (tripletCount == 0)
                        {
                            tripletCount = 2;
                        }
                        else
                        {
                            tripletCount++;
                        }
                        end = i + 5;
                    }
                    else
                    {
                        break;
                    }
                }
                if (end == -1)
                {
                    Debug.Log(CardType.c11122234 + "未找到三顺。");
                    return false;
                }

                if (cards.Count - tripletCount * 3 == tripletCount)
                {
                    datas = new List<CardData>()
                    {
                        cards[start],
                        cards[end]
                    };
                    AddListLog("判断 " + CardType.c11122234 + " 牌型传出数据", datas);
                    return true; ;
                }
                else
                {
                    Debug.Log(CardType.c11122234 + "三顺与单牌的数目不匹配，tripletCount = " + tripletCount);
                }
            }
            else
            {
                Debug.Log(CardType.c11122234 + "传入数据为空或数目不对。");
            }
            return false;
        }

        // Summary:
        //     判断飞机带队的牌型.
        //     datas
        //       第一个数据是飞机最小三张的一个牌的数据.
        //       第二个数据是飞机最大三张的一个排的数据.
        public bool IsSequenceOfTripletWithPair(List<CardData> cards, out List<CardData> datas)
        {
            datas = null;
            AddListLog("判断 " + CardType.c1112223344 + " 牌型传入数据", cards);
            if (cards != null && cards.Count >= 10 && cards.Count % 5 == 0)
            {
                cards.Sort();
                // 寻找三顺开始的位置.
                int start = -1;
                for (int i = 0; i < cards.Count - 3; i++)
                {
                    if (cards[i].value == cards[i + 1].value &&
                        cards[i].value == cards[i + 2].value)
                    {
                        start = i;
                        break;
                    }
                }
                if (start == -1)
                {
                    Debug.Log(CardType.c1112223344 + "未找到三张。");
                    return false;
                }
                // 寻找三顺结束位置.
                int end = -1;
                int tripletCount = 0;
                for (int i = start; i <= cards.Count - 6; i += 3)
                {
                    if (cards[i].value == cards[i + 1].value &&
                        cards[i].value == cards[i + 2].value &&
                        cards[i + 3].value == cards[i + 4].value &&
                        cards[i + 3].value == cards[i + 5].value &&
                        cards[i + 3].value - cards[i].value == 1)
                    {
                        if (tripletCount == 0)
                        {
                            tripletCount = 2;
                        }
                        else
                        {
                            tripletCount++;
                        }
                        end = i + 5;
                    }
                    else
                    {
                        break;
                    }
                }
                if (end == -1)
                {
                    Debug.Log(CardType.c1112223344 + "未找到三顺。");
                }

                // 三张的组数和对子的组数一样.
                if (cards.Count - tripletCount * 3 == tripletCount * 2)
                {
                    // 三顺前边存在对子.
                    if (start > 1)
                    {
                        for (int i = start - 1; i >= 1; i -= 2)
                        {
                            if (cards[i].value == cards[i - 1].value)
                            {

                            }
                            else
                            {
                                Console.WriteLine(CardType.c1112223344 + "不存在需要的对子1。start = " + start);
                                return false;
                            }
                        }
                    }
                    // 三顺后边存在对子.
                    if (end < cards.Count - 1)
                    {
                        for (int i = end + 1; i < cards.Count - 1; i += 2)
                        {
                            if (cards[i].value == cards[i + 1].value)
                            {

                            }
                            else
                            {
                                Console.WriteLine(CardType.c1112223344 + "不存在需要的对子2。end = " + end);
                                return false;
                            }
                        }
                    }
                    datas = new List<CardData>()
                    {
                        cards[start],
                        cards[end]
                    };
                    AddListLog("判断 " + CardType.c1112223344 + " 牌型传出数据", datas);
                    return true;
                }
                else
                {
                    Debug.Log(CardType.c1112223344 + "三顺与对子数目不匹配， tripletCount = " + tripletCount);
                }
            }
            else
            {
                Debug.Log(CardType.c1112223344 + "传入数据为空或数目不对。");
            }
            return false;
        }

        // Summary:
        //     四带两张单的牌型.
        //     datas
        //       一个四张的牌的数据.
        public bool IsQuaternionWithSingle(List<CardData> cards, out List<CardData> datas)
        {
            datas = null;
            AddListLog("判断 " + CardType.c412 + " 牌型传入数据", cards);
            if (cards != null && cards.Count == 6)
            {
                cards.Sort();

                // 寻找四张开始的位置.
                int start = -1;
                for (int i = 0; i <= cards.Count - 4; i++)
                {
                    if (cards[i].value == cards[i + 1].value &&
                        cards[i].value == cards[i + 2].value &&
                        cards[i].value == cards[i + 3].value)
                    {
                        start = i;
                        break;
                    }
                }

                if (start == -1)
                {
                    Debug.Log(CardType.c412 + "未找到四张。");
                    return false;
                }

                if (IsQuaternionWithPair(cards, out datas))
                {
                    Debug.Log(CardType.c412 + "传入数据值不对。");
                    return false;
                }

                datas = new List<CardData>()
                    {
                        cards[start]
                    };
                AddListLog("判断 " + CardType.c412 + " 牌型传出数据", datas);
                return true;
            }
            else
            {
                Debug.Log(CardType.c412 + "传入数据为空或数目不对。");
                return false;
            }
        }

        // Summary:
        //     四带一对牌型.
        //     datas
        //       一个四张的牌的数据.
        public bool IsQuaternionWithPair(List<CardData> cards, out List<CardData> datas)
        {
            AddListLog("判断 " + CardType.c411 + " 牌型传入数据", cards);
            datas = null;
            if (cards != null && cards.Count == 6)
            {
                // 如果四张带的对牌在List前边就从大到小排序,使对牌在List后边.
                if (cards[0].value == cards[1].value &&
                    cards[0].value != cards[2].value)
                {
                    cards.Reverse();
                }
                if (cards[0].value == cards[1].value &&
                    cards[0].value == cards[2].value &&
                    cards[0].value == cards[3].value &&
                    cards[4].value == cards[5].value)
                {
                    datas = new List<CardData>()
                    {
                        cards[0]
                    };
                    AddListLog("判断 " + CardType.c411 + " 牌型传出数据", datas);
                    return true;
                }
                else
                {
                    Debug.Log(CardType.c411 + "传入数据值不对。");
                }
            }
            else
            {
                Debug.Log(CardType.c411 + "传入数据为空或数目不对。");
            }
            return false;
        }

        public void AddListLog(string title, List<CardData> cardDatas)
        {
            if (cardDatas == null || cardDatas.Count < 1)
            {
                return;
            }
            string logString = "\r\n" + title + "开始####################################\r\n";
            foreach (CardData cardData in cardDatas)
            {
                logString += cardData.value + ", ";
            }
            logString += "\r\n" + title + "结束####################################\r\n";
            Debug.Log(logString);
        }

        public void AddListListLog(string title, List<List<GameObject>> cloness)
        {
            if (cloness == null || cloness.Count < 1)
            {
                return;
            }
            string logString = "\r\n" + title + "开始>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\r\n";
            int i = 0;
            foreach (List<GameObject> clones in cloness)
            {
                if (clones == null || clones.Count < 1)
                {
                    return;
                }
                logString += "\r\n" + i + " 开始:---------- \r\n";
                foreach (GameObject clone in clones)
                {
                    Card card = clone.GetComponent<Card>();
                    logString += card.value + ", ";
                }
                logString += "\r\n" + i + " 结束:---------- \r\n";
                i++;
            }
            logString += "\r\n" + title + "结束>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\r\n";
            Debug.Log(logString);
        }
    }
}
