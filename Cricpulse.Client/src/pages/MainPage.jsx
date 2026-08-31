function MainPage({ onRegister, onLogin }) {
  return (
    <>
      <nav className="navbar navbar-dark bg-dark">
        <div className="container">

          <span className="navbar-brand mb-0 h1">
            CricPulse
          </span>

          <div>
            <button className="btn btn-outline-light me-2"
            onClick={onLogin}> Login</button>

            <button
              className="btn btn-primary"
              onClick={onRegister}
            >
              Register
            </button>
          </div>

        </div>
      </nav>

      <main className="container text-center mt-5">

        <h1>Welcome to CricPulse 🏏</h1>

        <p className="text-muted">
          Your cricket scoring and player management platform.
        </p>

      </main>
    </>
  );
}

export default MainPage;