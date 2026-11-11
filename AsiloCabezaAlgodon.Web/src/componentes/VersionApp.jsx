// AC-15: versión y commit del frontend publicado (se fijan al compilar, ver vite.config.js)
function VersionApp() {
  const version = import.meta.env.VITE_VERSION || "sin versión";
  const commit = import.meta.env.VITE_COMMIT || "local";
  return <p className="version-app">Versión {version} · commit {commit}</p>;
}

export default VersionApp;
