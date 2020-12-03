using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitBullet : MonoBehaviour
{
    public GameObject bullet; // bullet prefab to spawn on split
    public Rigidbody rb; // rigidbody of parent
    public Vector3 location;

    public int splitsRemaining = 7; //Number of splits before object is destroyed on split
    RaycastHit hit;


    // Bounce bullet against single bounce wall
    void Bounce(RaycastHit hit)
    { 
        Vector3 wallNormal = hit.normal;
        Vector3 bulletTrajectory = rb.velocity;

        Vector3 newTrajectory = -bulletTrajectory.magnitude*(2 * Vector3.Dot(wallNormal, bulletTrajectory.normalized) * wallNormal - bulletTrajectory.normalized);

        rb.velocity = newTrajectory;
        rb.position += newTrajectory;
    }

    // Split bullet into two bullets
    void Split(RaycastHit hit)
    {
        Vector3 wallNormal = hit.normal;
        Vector3 bulletTrajectory = rb.velocity;
        Vector3 perpindicular = Vector3.Cross(bulletTrajectory.normalized, transform.forward);

        //Calculate new bullet trajectories
        Vector3 newTrajcetory1 = -bulletTrajectory.magnitude * Vector3.Normalize(bulletTrajectory.normalized + perpindicular);
        if (Vector3.Angle(newTrajcetory1, wallNormal) > 80)
            newTrajcetory1 = bulletTrajectory.magnitude * perpindicular.normalized;
        Vector3 newTrajcetory2 = -bulletTrajectory.magnitude * Vector3.Normalize(bulletTrajectory.normalized - perpindicular);
        if (Vector3.Angle(newTrajcetory2, wallNormal) > 80)
            newTrajcetory2 = bulletTrajectory.magnitude * -perpindicular.normalized;

        //Create copy bullets in new directions, decrease splits remaining
        GameObject bullet1 = Instantiate(bullet, transform.position, Quaternion.identity);
        bullet1.transform.position = transform.position + newTrajcetory1.normalized;
        bullet1.transform.rotation = Quaternion.Euler(90, 0, 0);
        bullet1.GetComponent<Rigidbody>().velocity = newTrajcetory1;
        bullet1.GetComponent<SplitBullet>().splitsRemaining = splitsRemaining - 1;

        GameObject bullet2 = Instantiate(bullet, transform.position, Quaternion.identity);
        bullet2.transform.position = transform.position + newTrajcetory2.normalized;
        bullet2.transform.rotation = Quaternion.Euler(90, 0, 0);
        bullet2.GetComponent<Rigidbody>().velocity = newTrajcetory2;
        bullet2.GetComponent<SplitBullet>().splitsRemaining = splitsRemaining - 1;

        Destroy(this.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        Physics.Raycast(transform.position, rb.velocity, out hit, 100f);
        if (other.gameObject.tag == "BigBullet")
        {
            Destroy(this.gameObject);
        }
        if (other.gameObject.tag == "Wall")
        {
            Destroy(this.gameObject);
        }
        if (other.gameObject.tag == "BounceWall")
        {
            Bounce(hit);
        }
        //Back out of wall before splitting (avoid repeated splits)
        if (other.gameObject.tag == "DoubleBounceWall")
        {
            rb.velocity = -rb.velocity;
        }
    }

    void OnTriggerExit(Collider other)
    {
        //Split or destory bullet
        if (other.gameObject.tag == "DoubleBounceWall")
        {
            if (splitsRemaining == 0)
            {
                Destroy(this.gameObject);
            }
            else
            {
                rb.velocity = -rb.velocity;
                Split(hit);
            }
        }
    }

}
