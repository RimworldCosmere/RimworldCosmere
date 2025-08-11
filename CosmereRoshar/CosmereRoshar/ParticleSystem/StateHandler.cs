using Object = UnityEngine.Object;

namespace Cosmere.Roshar.ParticleSystem;

public static class StateHandler {
    public static void DestroyParticleSystem(UnityEngine.ParticleSystem? particleSystem) {
        if (particleSystem is null) return;
        Object.Destroy(particleSystem.gameObject);
    }

    public static void RestoreParticleSystemState(
        UnityEngine.ParticleSystem? particleSystem
    ) {
        if (particleSystem is null) return;
        particleSystem.gameObject.SetActive(true);

        particleSystem.Play();

        UnityEngine.ParticleSystem.MainModule mainModule = particleSystem.main;
        mainModule.simulationSpeed = 1f;
    }

    public static void SetParticleSystemState(UnityEngine.ParticleSystem? particleSystem, bool isActive) {
        if (particleSystem is null) return;
        if (isActive) {
            particleSystem.gameObject.SetActive(true);
            particleSystem.Play();
        } else {
            particleSystem.gameObject.SetActive(false);
            particleSystem.Stop();
        }
    }
}