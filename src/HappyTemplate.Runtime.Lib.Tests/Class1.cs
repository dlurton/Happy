/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using NUnit.Framework;

namespace HappyTemplate.Runtime.Lib.Tests
{

    [TestFixture]
    public class QuickYamlTests
    {
        private const string example = @"---
receipt:     Oz-Ware Purchase Invoice
date:        2007-08-06
customer:
    given:   Dorothy
    family:  Gale

items:
    - part_no:   A4786
      descrip:   Water Bucket (Filled)
      price:     1.47
      quantity:  4

    - part_no:   E1628
      descrip:   High Heeled ""Ruby"" Slippers
      size:      8
      price:     100.27
      quantity:  1

bill_to:  &id001
    street: |
            123 Tornado Alley
            Suite 16
    city:   East Centerville
    state:  KS

ship_to:  *id001

specialDelivery:  >
    Follow the Yellow Brick
    Road to the Emerald City.
    Pay no attention to the
    man behind the curtain.";


        [Test]
        public void Test1()
        {
            dynamic dyaml = QuickYaml.FromString(example);

            //Assert.AreEqual("Oz-Ware Purchase Invoice", dyaml.receipt);
            //Assert.AreEqual("2007-08-06", dyaml.date);
            //Assert.AreEqual("Dorothy", dyaml.customer.given);
            //Assert.AreEqual("Gale", dyaml.customer.family);
            //Assert.AreEqual(2, dyaml._count);

            Console.WriteLine("receipt" + dyaml.receipt);
            Console.WriteLine("date" + dyaml.date);
            Console.WriteLine("given:" + dyaml.customer.given);
            Console.WriteLine("bill-to:" + dyaml.bill_to);
            Console.WriteLine("ship_to:" + dyaml.ship_to);
            Console.WriteLine("specialDelivery:" + dyaml.specialDelivery);
            Console.WriteLine("dyaml.items._count:" + dyaml.items._count);

            Console.WriteLine("bill_to:");
            writeAddress(dyaml.bill_to);

            Console.WriteLine("ship_to:");
            writeAddress(dyaml.ship_to);

            foreach(dynamic i in dyaml.items)
            {
                Console.WriteLine("item:");
                Console.WriteLine("\tpart_no:" + i.part_no);
                Console.WriteLine("\tdescrip:" + i.descrip);
                Console.WriteLine("\tprice:" + i.price);
                Console.WriteLine("\tquantity:" + i.quantity);
            }
        }

        private static void writeAddress(dynamic address)
        {
            Console.WriteLine("\tstreet:" + address.street);
            Console.WriteLine("\tcity:" + address.city);
            Console.WriteLine("\tstate:" + address.state);
        }
    }
}


