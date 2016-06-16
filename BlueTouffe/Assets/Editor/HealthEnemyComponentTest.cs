using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class HealthEnemyComponentTest {

	[Test]
	public void TestCreationEnemy()
	{
        GameObject GoTest = new GameObject();
        GoTest.AddComponent<HealthEnemy>();
        HealthEnemy HealthEnemy = GoTest.GetComponent<HealthEnemy>();

        Assert.AreEqual(HealthEnemy.MaxHealth, HealthEnemy.Health);
        Assert.AreEqual(false, HealthEnemy.IsDead);
    }

    [Test]
    public void TestDamageEnemy()
    {
        GameObject GoTest = new GameObject();
        GoTest.AddComponent<HealthEnemy>();
        HealthEnemy HealthEnemy = GoTest.GetComponent<HealthEnemy>();

        Assert.AreEqual( HealthEnemy.MaxHealth, HealthEnemy.Health );

        HealthEnemy.ReceiveDamage( 10 );
        Assert.AreEqual( HealthEnemy.MaxHealth - 10, HealthEnemy.Health );

        HealthEnemy.ReceiveDamage( -10 );
        Assert.AreEqual( HealthEnemy.MaxHealth - 10, HealthEnemy.Health );

        HealthEnemy.ReceiveDamage( HealthEnemy.MaxHealth );
        Assert.AreEqual( 0, HealthEnemy.Health );
    }

    [Test]
    public void TestDyingEnemy()
    {
        GameObject GoTest = new GameObject();
        GoTest.AddComponent<HealthEnemy>();
        HealthEnemy HealthEnemy = GoTest.GetComponent<HealthEnemy>();

        Assert.AreEqual( false, HealthEnemy.IsDead );

        HealthEnemy.ReceiveDamage( HealthEnemy.MaxHealth );
        Assert.AreEqual( true, HealthEnemy.IsDead );
    }
}
