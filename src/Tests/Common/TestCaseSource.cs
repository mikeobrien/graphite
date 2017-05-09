using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Tests.Common
{
    public class TestCaseSource
    {
        public static object[][] Create(
            Action<CaseDsl> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl(cases));
            return cases.ToArray();
        }

        public static object[][] Create<T>(
            Action<CaseDsl<T>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<T>(cases));
            return cases.ToArray();
        }

        public static object[][] CreateWithExpression<T>(
            Action<CaseDsl<Expression<Func<T, object>>>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<Expression<Func<T, object>>>(cases));
            return cases.ToArray();
        }

        public static object[][] CreateWithExpression<T>(
            Action<CaseDsl<Expression<Action<T>>>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<Expression<Action<T>>>(cases));
            return cases.ToArray();
        }

        public static object[][] Create<T1, T2>(
            Action<CaseDsl<T1, T2>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<T1, T2>(cases));
            return cases.ToArray();
        }

        public static object[][] CreateWithExpression<T1, T2>(
            Action<CaseDsl<Expression<Func<T1, object>>, T2>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<Expression<Func<T1, object>>, T2>(cases));
            return cases.ToArray();
        }

        public static object[][] CreateWithExpression<T1, T2>(
            Action<CaseDsl<Expression<Action<T1>>, T2>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<Expression<Action<T1>>, T2>(cases));
            return cases.ToArray();
        }

        public static object[][] CreateWithExpressions<T1, T2>(
            Action<CaseDsl<Expression<Func<T1, object>>, Expression<Func<T2, object>>>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<Expression<Func<T1, object>>, Expression<Func<T2, object>>>(cases));
            return cases.ToArray();
        }

        public static object[][] CreateWithExpressions<T1, T2, T3>(
            Action<CaseDsl<Expression<Func<T1, object>>, Expression<Func<T2, object>>, T3>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<Expression<Func<T1, object>>, Expression<Func<T2, object>>, T3>(cases));
            return cases.ToArray();
        }

        public static object[][] Create<T1, T2, T3>(
            Action<CaseDsl<T1, T2, T3>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<T1, T2, T3>(cases));
            return cases.ToArray();
        }

        public static object[][] CreateWithExpression<T1, T2, T3>(
            Action<CaseDsl<Expression<Func<T1, object>>, T2, T3>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<Expression<Func<T1, object>>, T2, T3>(cases));
            return cases.ToArray();
        }

        public static object[][] CreateWithExpression<T1, T2, T3>(
            Action<CaseDsl<Expression<Action<T1>>, T2, T3>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<Expression<Action<T1>>, T2, T3>(cases));
            return cases.ToArray();
        }

        public static object[][] Create<T1, T2, T3, T4>(
            Action<CaseDsl<T1, T2, T3, T4>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<T1, T2, T3, T4>(cases));
            return cases.ToArray();
        }

        public static object[][] CreateWithExpression<T1, T2, T3, T4>(
            Action<CaseDsl<Expression<Func<T1, object>>, T2, T3, T4>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<Expression<Func<T1, object>>, T2, T3, T4>(cases));
            return cases.ToArray();
        }

        public static object[][] Create<T1, T2, T3, T4, T5>(
            Action<CaseDsl<T1, T2, T3, T4, T5>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<T1, T2, T3, T4, T5>(cases));
            return cases.ToArray();
        }

        public static object[][] CreateWithExpression<T1, T2, T3, T4, T5>(
            Action<CaseDsl<Expression<Func<T1, object>>, T2, T3, T4, T5>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<Expression<Func<T1, object>>, T2, T3, T4, T5>(cases));
            return cases.ToArray();
        }

        public static object[][] Create<T1, T2, T3, T4, T5, T6>(
            Action<CaseDsl<T1, T2, T3, T4, T5, T6>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<T1, T2, T3, T4, T5, T6>(cases));
            return cases.ToArray();
        }

        public static object[][] CreateWithExpression<T1, T2, T3, T4, T5, T6>(
            Action<CaseDsl<Expression<Func<T1, object>>, T2, T3, T4, T5, T6>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<Expression<Func<T1, object>>, T2, T3, T4, T5, T6>(cases));
            return cases.ToArray();
        }

        public static object[][] Create<T1, T2, T3, T4, T5, T6, T7>(
            Action<CaseDsl<T1, T2, T3, T4, T5, T6, T7>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<T1, T2, T3, T4, T5, T6, T7>(cases));
            return cases.ToArray();
        }

        public static object[][] CreateWithExpression<T1, T2, T3, T4, T5, T6, T7>(
            Action<CaseDsl<Expression<Func<T1, object>>, T2, T3, T4, T5, T6, T7>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<Expression<Func<T1, object>>, T2, T3, T4, T5, T6, T7>(cases));
            return cases.ToArray();
        }

        public static object[][] Create<T1, T2, T3, T4, T5, T6, T7, T8>(
            Action<CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8>(cases));
            return cases.ToArray();
        }

        public static object[][] CreateWithExpression<T1, T2, T3, T4, T5, T6, T7, T8>(
            Action<CaseDsl<Expression<Func<T1, object>>, T2, T3, T4, T5, T6, T7, T8>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<Expression<Func<T1, object>>, T2, T3, T4, T5, T6, T7, T8>(cases));
            return cases.ToArray();
        }

        public static object[][] Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Action<CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9>(cases));
            return cases.ToArray();
        }

        public static object[][] CreateWithExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Action<CaseDsl<Expression<Func<T1, object>>, T2, T3, T4, T5, T6, T7, T8, T9>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<Expression<Func<T1, object>>, T2, T3, T4, T5, T6, T7, T8, T9>(cases));
            return cases.ToArray();
        }

        public static object[][] Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Action<CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(cases));
            return cases.ToArray();
        }

        public static object[][] CreateWithExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Action<CaseDsl<Expression<Func<T1, object>>, T2, T3, T4, T5, T6, T7, T8, T9, T10>> config)
        {
            var cases = new List<object[]>();
            config(new CaseDsl<Expression<Func<T1, object>>, T2, T3, T4, T5, T6, T7, T8, T9, T10>(cases));
            return cases.ToArray();
        }

        public class CaseDslBase
        {
            private readonly List<object[]> _cases;

            public CaseDslBase(List<object[]> cases)
            {
                _cases = cases;
            }

            protected void AddCase(params object[] cases)
            {
                _cases.Add(cases);
            }
        }

        public class CaseDsl : CaseDslBase
        {
            public CaseDsl(List<object[]> cases) : base(cases) { }

            public CaseDsl Add<TType>()
            {
                AddCase(typeof(TType));
                return this;
            }

            public CaseDsl Add<TType>(TType value)
            {
                AddCase(typeof(TType), value);
                return this;
            }

            public CaseDsl Add<TType1, TType2>()
            {
                AddCase(typeof(TType1), typeof(TType2));
                return this;
            }

            public CaseDsl Add<TType1, TType2>(TType1 valueA, TType2 valueB)
            {
                AddCase(typeof(TType1), typeof(TType2), valueA, valueB);
                return this;
            }

            public CaseDsl Add<TArg>(Expression<Func<TArg, object>> expression)
            {
                AddCase(expression);
                return this;
            }

            public CaseDsl Add<TArg1, TArg2>
                (Expression<Func<TArg1, object>> expression1,
                Expression<Func<TArg2, object>> expression2)
            {
                AddCase(expression1, expression2);
                return this;
            }
        }

        public class CaseDsl<T> : CaseDslBase
        {
            public CaseDsl(List<object[]> cases) : base(cases) { }

            public CaseDsl<T> Add(T value)
            {
                AddCase(value);
                return this;
            }

            public CaseDsl<T> Add<TType>(T value)
            {
                AddCase(typeof(TType), value);
                return this;
            }

            public CaseDsl<T> Add<TType>(TType valueA, T value)
            {
                AddCase(typeof(TType), valueA, value);
                return this;
            }

            public CaseDsl<T> Add<TType1, TType2>(T value)
            {
                AddCase(typeof(TType1), typeof(TType2), value);
                return this;
            }

            public CaseDsl<T> Add<TType1, TType2>(
                TType1 valueA, TType2 valueB, T value)
            {
                AddCase(typeof(TType1), typeof(TType2), valueA, valueB, value);
                return this;
            }

            public CaseDsl<T> Add<TArg>(Expression<Func<TArg, object>> expression, T value)
            {
                AddCase(expression, value);
                return this;
            }

            public CaseDsl<T> Add<TArg1, TArg2>
                (Expression<Func<TArg1, object>> expression1,
                Expression<Func<TArg2, object>> expression2, T value)
            {
                AddCase(expression1, expression2, value);
                return this;
            }
        }

        public class CaseDsl<T1, T2> : CaseDslBase
        {
            public CaseDsl(List<object[]> cases) : base(cases) { }

            public CaseDsl<T1, T2> Add(T1 value1, T2 value2)
            {
                AddCase(value1, value2);
                return this;
            }

            public CaseDsl<T1, T2> Add<TType>(T1 value1, T2 value2)
            {
                AddCase(typeof(TType), value1, value2);
                return this;
            }

            public CaseDsl<T1, T2> Add<TType>(
                TType valueA, T1 value1, T2 value2)
            {
                AddCase(typeof(TType), valueA, value1, value2);
                return this;
            }

            public CaseDsl<T1, T2> Add<TType1, TType2>(
                T1 value1, T2 value2)
            {
                AddCase(typeof(TType1), typeof(TType2),
                    value1, value2);
                return this;
            }

            public CaseDsl<T1, T2> Add<TType1, TType2>(TType1 valueA,
                TType2 valueB, T1 value1, T2 value2)
            {
                AddCase(typeof(TType1), typeof(TType2), valueA,
                    valueB, value1, value2);
                return this;
            }

            public CaseDsl<T1, T2> Add<TArg>(
                Expression<Func<TArg, object>> expression,
                T1 value1, T2 value2)
            {
                AddCase(expression, value1, value2);
                return this;
            }

            public CaseDsl<T1, T2> Add<TArg1, TArg2>
                (Expression<Func<TArg1, object>> expression1,
                Expression<Func<TArg2, object>> expression2,
                T1 value1, T2 value2)
            {
                AddCase(expression1, expression2, value1, value2);
                return this;
            }
        }

        public class CaseDsl<T1, T2, T3> : CaseDslBase
        {
            public CaseDsl(List<object[]> cases) : base(cases) { }

            public CaseDsl<T1, T2, T3> Add(T1 value1, T2 value2, T3 value3)
            {
                AddCase(value1, value2, value3);
                return this;
            }

            public CaseDsl<T1, T2, T3> Add<TType>(T1 value1, T2 value2, T3 value3)
            {
                AddCase(typeof(TType), value1, value2, value3);
                return this;
            }

            public CaseDsl<T1, T2, T3> Add<TType>(
                TType valueA, T1 value1, T2 value2, T3 value3)
            {
                AddCase(typeof(TType), valueA, value1, value2, value3);
                return this;
            }

            public CaseDsl<T1, T2, T3> Add<TType1, TType2>(
                T1 value1, T2 value2, T3 value3)
            {
                AddCase(typeof(TType1), typeof(TType2),
                    value1, value2, value3);
                return this;
            }

            public CaseDsl<T1, T2, T3> Add<TType1, TType2>(
                TType1 valueA, TType2 valueB,
                T1 value1, T2 value2, T3 value3)
            {
                AddCase(typeof(TType1), typeof(TType2), valueA,
                    valueB, value1, value2, value3);
                return this;
            }

            public CaseDsl<T1, T2, T3> Add<TArg>(
                Expression<Func<TArg, object>> expression,
                T1 value1, T2 value2, T3 value3)
            {
                AddCase(expression, value1, value2, value3);
                return this;
            }

            public CaseDsl<T1, T2, T3> Add(
                Expression<Action<T1>> expression,
                T2 value2, T3 value3)
            {
                AddCase(expression, value2, value3);
                return this;
            }

            public CaseDsl<T1, T2, T3> Add<TArg1, TArg2>
                (Expression<Func<TArg1, object>> expression1,
                Expression<Func<TArg2, object>> expression2,
                T1 value1, T2 value2, T3 value3)
            {
                AddCase(expression1, expression2, value1, value2, value3);
                return this;
            }
        }

        public class CaseDsl<T1, T2, T3, T4> : CaseDslBase
        {
            public CaseDsl(List<object[]> cases) : base(cases) { }

            public CaseDsl<T1, T2, T3, T4> Add(T1 value1,
                T2 value2, T3 value3, T4 value4)
            {
                AddCase(value1, value2, value3, value4);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4> Add<TType>(T1 value1,
                T2 value2, T3 value3, T4 value4)
            {
                AddCase(typeof(TType), value1, value2, value3, value4);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4> Add<TType>(
                TType valueA, T1 value1, T2 value2,
                T3 value3, T4 value4)
            {
                AddCase(typeof(TType), valueA, value1,
                    value2, value3, value4);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4> Add<TType1, TType2>(
                T1 value1, T2 value2, T3 value3, T4 value4)
            {
                AddCase(typeof(TType1), typeof(TType2), value1,
                    value2, value3, value4);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4> Add<TType1, TType2>(
                TType1 valueA, TType2 valueB, T1 value1, T2 value2,
                T3 value3, T4 value4)
            {
                AddCase(typeof(TType1), typeof(TType2), valueA,
                    valueB, value1, value2, value3, value4);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4> Add<TArg>(
                Expression<Func<TArg, object>> expression,
                T1 value1, T2 value2, T3 value3, T4 value4)
            {
                AddCase(expression, value1, value2, value3, value4);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4> Add<TArg1, TArg2>
                (Expression<Func<TArg1, object>> expression1,
                Expression<Func<TArg2, object>> expression2,
                T1 value1, T2 value2, T3 value3, T4 value4)
            {
                AddCase(expression1, expression2, value1, value2,
                    value3, value4);
                return this;
            }
        }

        public class CaseDsl<T1, T2, T3, T4, T5> : CaseDslBase
        {
            public CaseDsl(List<object[]> cases) : base(cases) { }

            public CaseDsl<T1, T2, T3, T4, T5> Add(T1 value1, T2 value2,
                T3 value3, T4 value4, T5 value5)
            {
                AddCase(value1, value2, value3, value4, value5);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5> Add<TType>(T1 value1,
                T2 value2, T3 value3, T4 value4, T5 value5)
            {
                AddCase(typeof(TType), value1, value2, value3, value4, value5);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5> Add<TType>(
                TType valueA, T1 value1, T2 value2, T3 value3,
                T4 value4, T5 value5)
            {
                AddCase(typeof(TType), valueA, value1, value2,
                    value3, value4, value5);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5> Add<TType1, TType2>(
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
            {
                AddCase(typeof(TType1), typeof(TType2), value1,
                    value2, value3, value4, value5);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5> Add<TType1, TType2>(
                TType1 valueA, TType2 valueB, T1 value1, T2 value2,
                T3 value3, T4 value4, T5 value5)
            {
                AddCase(typeof(TType1), typeof(TType2), valueA,
                    valueB, value1, value2, value3, value4, value5);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5> Add<TArg>(
                Expression<Func<TArg, object>> expression,
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
            {
                AddCase(expression, value1, value2, value3, value4, value5);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5> Add<TArg1, TArg2>
                (Expression<Func<TArg1, object>> expression1,
                Expression<Func<TArg2, object>> expression2,
                T1 value1, T2 value2, T3 value3, T4 value4,
                T5 value5)
            {
                AddCase(expression1, expression2, value1, value2,
                    value3, value4, value5);
                return this;
            }
        }

        public class CaseDsl<T1, T2, T3, T4, T5, T6> : CaseDslBase
        {
            public CaseDsl(List<object[]> cases) : base(cases) { }

            public CaseDsl<T1, T2, T3, T4, T5, T6> Add(T1 value1, T2 value2,
                T3 value3, T4 value4, T5 value5, T6 value6)
            {
                AddCase(value1, value2, value3, value4, value5, value6);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6> Add<TType>(T1 value1, T2 value2,
                T3 value3, T4 value4, T5 value5, T6 value6)
            {
                AddCase(typeof(TType), value1, value2, value3, value4, value5, value6);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6> Add<TType>(
                TType valueA, T1 value1, T2 value2, T3 value3,
                T4 value4, T5 value5, T6 value6)
            {
                AddCase(typeof(TType), valueA, value1, value2,
                    value3, value4, value5, value6);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6> Add<TType1, TType2>(
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6)
            {
                AddCase(typeof(TType1), typeof(TType2), value1,
                    value2, value3, value4, value5, value6);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6> Add<TType1, TType2>(
                TType1 valueA, TType2 valueB, T1 value1, T2 value2,
                T3 value3, T4 value4, T5 value5, T6 value6)
            {
                AddCase(typeof(TType1), typeof(TType2), valueA, valueB,
                    value1, value2, value3, value4, value5, value6);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6> Add<TArg>(
                Expression<Func<TArg, object>> expression,
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
            {
                AddCase(expression, value1, value2, value3, value4, value5, value6);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6> Add<TArg1, TArg2>
                (Expression<Func<TArg1, object>> expression1,
                Expression<Func<TArg2, object>> expression2,
                T1 value1, T2 value2, T3 value3, T4 value4,
                T5 value5, T6 value6)
            {
                AddCase(expression1, expression2, value1, value2,
                    value3, value4, value5, value6);
                return this;
            }
        }

        public class CaseDsl<T1, T2, T3, T4, T5, T6, T7> : CaseDslBase
        {
            public CaseDsl(List<object[]> cases) : base(cases) { }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7> Add(T1 value1, T2 value2,
                T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
            {
                AddCase(value1, value2, value3, value4, value5, value6, value7);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7> Add<TType>(T1 value1,
                T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
            {
                AddCase(typeof(TType), value1, value2,
                    value3, value4, value5, value6, value7);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7> Add<TType>(
                TType valueA, T1 value1, T2 value2, T3 value3,
                T4 value4, T5 value5, T6 value6, T7 value7)
            {
                AddCase(typeof(TType), valueA, value1, value2,
                    value3, value4, value5, value6, value7);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7> Add<TType1, TType2>(
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7)
            {
                AddCase(typeof(TType1), typeof(TType2), value1, value2, value3, value4, value5,
                    value6, value7);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7> Add<TType1, TType2>(
                TType1 valueA, TType2 valueB, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7)
            {
                AddCase(typeof(TType1), typeof(TType2), valueA, valueB, value1, value2,
                    value3, value4, value5, value6, value7);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7> Add<TArg>(
                Expression<Func<TArg, object>> expression, T1 value1, T2 value2,
                T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
            {
                AddCase(expression, value1, value2, value3,
                    value4, value5, value6, value7);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7> Add<TArg1, TArg2>
                (Expression<Func<TArg1, object>> expression1,
                Expression<Func<TArg2, object>> expression2,
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7)
            {
                AddCase(expression1, expression2, value1, value2, value3, value4, value5,
                    value6, value7);
                return this;
            }
        }

        public class CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8> : CaseDslBase
        {
            public CaseDsl(List<object[]> cases) : base(cases) { }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8> Add(
                T1 value1, T2 value2, T3 value3, T4 value4,
                T5 value5, T6 value6, T7 value7, T8 value8)
            {
                AddCase(value1, value2, value3, value4,
                    value5, value6, value7, value8);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8> Add<TType>(
                T1 value1, T2 value2, T3 value3, T4 value4,
                T5 value5, T6 value6, T7 value7, T8 value8)
            {
                AddCase(typeof(TType), value1, value2, value3,
                    value4, value5, value6, value7, value8);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8> Add<TType>(
                TType valueA, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8)
            {
                AddCase(typeof(TType), valueA, value1, value2, value3, value4, value5,
                    value6, value7, value8);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8> Add<TType1, TType2>(
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8)
            {
                AddCase(typeof(TType1), typeof(TType2), value1, value2, value3, value4, value5,
                    value6, value7, value8);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8> Add<TType1, TType2>(
                TType1 valueA, TType2 valueB, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8)
            {
                AddCase(typeof(TType1), typeof(TType2), valueA, valueB, value1, value2,
                    value3, value4, value5, value6, value7, value8);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8> Add<TArg>(
                Expression<Func<TArg, object>> expression,
                T1 value1, T2 value2, T3 value3, T4 value4,
                T5 value5, T6 value6, T7 value7, T8 value8)
            {
                AddCase(expression, value1, value2, value3,
                    value4, value5, value6, value7, value8);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8> Add<TArg1, TArg2>
                (Expression<Func<TArg1, object>> expression1,
                Expression<Func<TArg2, object>> expression2,
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8)
            {
                AddCase(expression1, expression2, value1, value2, value3, value4, value5,
                    value6, value7, value8);
                return this;
            }
        }

        public class CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9> : CaseDslBase
        {
            public CaseDsl(List<object[]> cases) : base(cases) { }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9> Add(
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8, T9 value9)
            {
                AddCase(value1, value2, value3, value4,
                    value5, value6, value7, value8, value9);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9> Add<TType>(
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8, T9 value9)
            {
                AddCase(typeof(TType), value1, value2, value3, value4,
                    value5, value6, value7, value8, value9);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9> Add<TType>(
                TType valueA, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8, T9 value9)
            {
                AddCase(typeof(TType), valueA, value1, value2, value3, value4, value5,
                    value6, value7, value8, value9);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9> Add<TType1, TType2>(
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8, T9 value9)
            {
                AddCase(typeof(TType1), typeof(TType2), value1, value2, value3, value4, value5,
                    value6, value7, value8, value9);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9> Add<TType1, TType2>(
                TType1 valueA, TType2 valueB, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8, T9 value9)
            {
                AddCase(typeof(TType1), typeof(TType2), valueA, valueB, value1, value2,
                    value3, value4, value5, value6, value7, value8, value9);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9> Add<TArg>(
                Expression<Func<TArg, object>> expression,
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8, T9 value9)
            {
                AddCase(expression, value1, value2, value3, value4,
                    value5, value6, value7, value8, value9);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9> Add<TArg1, TArg2>
                (Expression<Func<TArg1, object>> expression1,
                Expression<Func<TArg2, object>> expression2,
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8, T9 value9)
            {
                AddCase(expression1, expression2, value1, value2, value3, value4, value5,
                    value6, value7, value8, value9);
                return this;
            }
        }

        public class CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : CaseDslBase
        {
            public CaseDsl(List<object[]> cases) : base(cases) { }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Add(
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8, T9 value9, T10 value10)
            {
                AddCase(value1, value2, value3, value4, value5,
                    value6, value7, value8, value9, value10);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Add<TType>(
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8, T9 value9, T10 value10)
            {
                AddCase(typeof(TType), value1, value2, value3, value4, value5,
                    value6, value7, value8, value9, value10);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Add<TType>(
                TType valueA, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8, T9 value9, T10 value10)
            {
                AddCase(typeof(TType), valueA, value1, value2, value3, value4, value5,
                    value6, value7, value8, value9, value10);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Add<TType1, TType2>(
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8, T9 value9, T10 value10)
            {
                AddCase(typeof(TType1), typeof(TType2), value1, value2, value3, value4, value5,
                    value6, value7, value8, value9, value10);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Add<TType1, TType2>(
                TType1 valueA, TType2 valueB, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8, T9 value9, T10 value10)
            {
                AddCase(typeof(TType1), typeof(TType2), valueA, valueB, value1, value2,
                    value3, value4, value5, value6, value7, value8, value9, value10);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Add<TArg>
                (Expression<Func<TArg, object>> expression,
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8, T9 value9, T10 value10)
            {
                AddCase(expression, value1, value2, value3, value4, value5,
                    value6, value7, value8, value9, value10);
                return this;
            }

            public CaseDsl<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Add<TArg1, TArg2>
                (Expression<Func<TArg1, object>> expression1,
                Expression<Func<TArg2, object>> expression2,
                T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,
                T6 value6, T7 value7, T8 value8, T9 value9, T10 value10)
            {
                AddCase(expression1, expression2, value1, value2, value3, value4, value5,
                    value6, value7, value8, value9, value10);
                return this;
            }
        }
    }
}
