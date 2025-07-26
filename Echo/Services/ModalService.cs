using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo.Services;

public  class ModalService
{


    public event Action<Modals> ModalChanged;
    private Modals _currentModal = Modals.None;
    public Modals CurrentModal
    {
        get => _currentModal;
        set
        {
            if (_currentModal != value)
            {
                _currentModal = value;
                ModalChanged?.Invoke(_currentModal);
            }
        }
    }
    public void OpenModal(Modals modal)
    {
        CurrentModal = modal;
    }
    public void CloseModal()
    {
        CurrentModal = Modals.None;
    }



}


public enum Modals
{
    None,
    Settings,
    SelectSteamGame
}
