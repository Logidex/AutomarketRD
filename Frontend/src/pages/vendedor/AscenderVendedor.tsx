import AscenderRol from "../../components/AscenderRol";

export default function AscenderVendedor() {
  return (
    <div className="mx-auto max-w-xl">
      <h1 className="text-2xl font-bold text-ink">
        Escalar a cuenta Dealer
      </h1>
      <p className="mt-1 text-sm text-ink-3">
        Como Vendedor puedes escalar tu cuenta a Dealer cuando lo necesites.
        Esta acción no puede revertirse por tu cuenta.
      </p>

      <div className="mt-6 flex flex-col gap-3 rounded-xl border border-line bg-surface-2 p-6 shadow-sm text-sm text-ink-2">
        <AscenderRol />
      </div>
    </div>
  );
}